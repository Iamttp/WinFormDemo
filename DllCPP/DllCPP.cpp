// DllCPP.cpp : 定义 DLL 应用程序的导出函数。
//

#include "stdafx.h"

#include<opencv.hpp>
#include<opencv2\highgui\highgui.hpp>
#include<opencv2\opencv.hpp>

using namespace cv;
//未使用
extern "C" __declspec(dllexport) int ImgRead(const char* foldpath)//图像匹配
{
	//【1】从摄像头读入视频
	static VideoCapture capture(0);
	Mat frame; //定义一个Mat变量，用于存储每一帧的图像
	capture >> frame;  //读取当前帧    
	cv::imwrite(foldpath, frame);
	return capture.isOpened();
}


#include<opencv2\opencv.hpp>
#include<vector>
using namespace cv;
using namespace std;

extern "C" __declspec(dllexport) double ImgCmp(const char* srcPath, const char* dstPath, const char* saveGrayPath, const char* saveDstPath, int myX = 300, int myY = 486)
{
	Mat src1, src2, dst;
	src1 = imread(srcPath, IMREAD_GRAYSCALE);
	src2 = imread(dstPath, IMREAD_GRAYSCALE);
	resize(src1, src1, Size(myX, myY));
	resize(src2, src2, Size(myX, myY));

	//1.AKAZE
	Ptr<AKAZE>akaze = AKAZE::create();
	vector<KeyPoint> keypoints1, keypoints2;
	Mat descriptors1, descriptors2;
	akaze->detectAndCompute(src1, Mat(), keypoints1, descriptors1);
	akaze->detectAndCompute(src2, Mat(), keypoints2, descriptors2);

	//2.暴力匹配
	BFMatcher matcher;
	vector<DMatch>matches;
	matcher.match(descriptors1, descriptors2, matches);

	double minDist = 1000;
	for (int i = 0; i < descriptors1.rows; i++)
	{
		double dist = matches[i].distance;
		if (dist < minDist)
		{
			minDist = dist;
		}
	}
	//3.挑选较好的匹配DMatch
	vector<DMatch>goodMatches;
	for (int i = 0; i < descriptors1.rows; i++)
	{
		double dist = matches[i].distance;
		if (dist < max(1.5 * minDist, 0.02))
		{
			goodMatches.push_back(matches[i]);
		}
	}
	//4.绘制saveGrayPath路径的good_match_img
	Mat good_match_img;
	drawMatches(src1, keypoints1, src2, keypoints2, goodMatches, good_match_img, Scalar::all(-1), Scalar::all(-1), vector<char>(), DrawMatchesFlags::NOT_DRAW_SINGLE_POINTS);

	//perspective transform
	vector<Point2f>src1GoodPoints;
	vector<Point2f>src2GoodPoints;
	for (int i = 0; i < goodMatches.size(); i++)
	{
		src1GoodPoints.push_back(keypoints1[goodMatches[i].queryIdx].pt);
		src2GoodPoints.push_back(keypoints2[goodMatches[i].trainIdx].pt);
	}
	Mat P = findHomography(src1GoodPoints, src2GoodPoints, RANSAC);//有不良匹配点时用RANSAC
	vector<Point2f>src1corner(4);
	vector<Point2f>src2corner(4);
	src1corner[0] = Point(0, 0);
	src1corner[1] = Point(src1.cols, 0);
	src1corner[2] = Point(src1.cols, src1.rows);
	src1corner[3] = Point(0, src1.rows);
	perspectiveTransform(src1corner, src2corner, P);
	//在匹配图上画
	line(good_match_img, Point(src2corner[0].x + src1.cols, src2corner[0].y), Point(src2corner[1].x + src1.cols, src2corner[1].y), Scalar(0, 0, 255), 2);
	line(good_match_img, Point(src2corner[1].x + src1.cols, src2corner[1].y), Point(src2corner[2].x + src1.cols, src2corner[2].y), Scalar(0, 0, 255), 2);
	line(good_match_img, Point(src2corner[2].x + src1.cols, src2corner[2].y), Point(src2corner[3].x + src1.cols, src2corner[3].y), Scalar(0, 0, 255), 2);
	line(good_match_img, Point(src2corner[3].x + src1.cols, src2corner[3].y), Point(src2corner[0].x + src1.cols, src2corner[0].y), Scalar(0, 0, 255), 2);

	//在原图上画
	Mat srcRes = imread(dstPath);
	resize(srcRes, srcRes, Size(myX, myY));
	line(srcRes, src2corner[0], src2corner[1], Scalar(0, 0, 255), 2);
	line(srcRes, src2corner[1], src2corner[2], Scalar(0, 0, 255), 2);
	line(srcRes, src2corner[2], src2corner[3], Scalar(0, 0, 255), 2);
	line(srcRes, src2corner[3], src2corner[0], Scalar(0, 0, 255), 2);

	imwrite(saveGrayPath, good_match_img);
	imwrite(saveDstPath, srcRes);

	return minDist;
}

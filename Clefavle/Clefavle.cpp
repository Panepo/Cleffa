// Clefavle.cpp : 定義主控台應用程式的進入點。
//

#include "stdafx.h"

#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/features2d/features2d.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/video/video.hpp>
#include <iostream>

using namespace cv;
using namespace std;

class Tracker {
	vector<Point2f> trackedFeatures;
	Mat             prevGray;
public:
	bool            freshStart;
	Mat_<float>     rigidTransform;

	Tracker() :freshStart(true) {
		rigidTransform = Mat::eye(3, 3, CV_32FC1); //affine 2x3 in a 3x3 matrix
	}

	void processImage(Mat& img) {
		Mat gray;
		cvtColor(img, gray, CV_BGR2GRAY);

		//int startX = 200, startY = 200, width = 100, height = 100;
		//Mat ROI(gray, Rect(startX, startY, width, height));

		vector<Point2f> corners;
		if (trackedFeatures.size() < 250) {
			goodFeaturesToTrack(gray, corners, 300, 0.01, 10);
			cout << "found " << corners.size() << " features\n";
			for (int i = 0; i < corners.size(); ++i) {
				trackedFeatures.push_back(corners[i]);
			}
		}

		if (!prevGray.empty()) {
			vector<uchar> status; vector<float> errors;
			calcOpticalFlowPyrLK(prevGray, gray, trackedFeatures, corners, status, errors, Size(10, 10));

			if (countNonZero(status) < status.size() * 0.8) {
				cout << "cataclysmic error \n";
				rigidTransform = Mat::eye(3, 3, CV_32FC1);
				trackedFeatures.clear();
				prevGray.release();
				freshStart = true;
				return;
			}
			else
				freshStart = false;

			Mat_<float> newRigidTransform = estimateRigidTransform(trackedFeatures, corners, false);
			Mat_<float> nrt33 = Mat_<float>::eye(3, 3);
			newRigidTransform.copyTo(nrt33.rowRange(0, 2));
			rigidTransform *= nrt33;

			trackedFeatures.clear();
			for (int i = 0; i < status.size(); ++i) {
				if (status[i]) {
					trackedFeatures.push_back(corners[i]);
				}
			}
		}

		for (int i = 0; i < trackedFeatures.size(); ++i) {
			circle(img, trackedFeatures[i], 3, Scalar(0, 255, 0), CV_FILLED);
		}

		gray.copyTo(prevGray);
	}
};


int main() {
	VideoCapture vc(2);

	vc.set(CAP_PROP_FRAME_WIDTH, 1920);
	vc.set(CAP_PROP_FRAME_HEIGHT, 1080);	

	Mat frame, orig, orig_warped, tmp;

	Tracker tracker;

	while (vc.isOpened()) {
		vc >> frame;
		if (frame.empty()) break;
		frame.copyTo(orig);

		//Mat output;
		//resize(orig, output, Size(), 0.5, 0.5);
		//imshow("Original", output);

		tracker.processImage(orig);

		Mat invTrans = tracker.rigidTransform.inv(DECOMP_SVD);
		warpAffine(orig, orig_warped, invTrans.rowRange(0, 2), Size());

		Mat output_warped;
		resize(orig_warped, output_warped, Size(), 0.5, 0.5);
		imshow("Clefavle Stablizer", output_warped);

		int getKey = waitKey(10);
		if (getKey == 27)
			break;
		else if (getKey == ' ')
			break;
		else if (getKey == 'q')
			break;
	}
	vc.release();

}
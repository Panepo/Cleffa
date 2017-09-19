#pragma once
#ifndef BARCODEFILTER_H
#define BARCODEFILTER_H

#include <opencv2\imgproc\types_c.h>
#include <opencv2\imgcodecs\imgcodecs.hpp>
#include <opencv2\core\core.hpp>
#include <opencv2\imgproc\imgproc.hpp>
#include <opencv2\objdetect.hpp>
#include <Robuffer.h>

using namespace std;
using namespace cv;

using namespace Windows::Graphics::Imaging;
using namespace Windows::UI::Xaml::Media::Imaging;
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;

namespace Clefable
{
    public ref class barcodeFilter sealed
    {
	public:
		barcodeFilter();
		WriteableBitmap^ processFilterBitmap(WriteableBitmap^ input);
		WriteableBitmap^ processFilterTest(WriteableBitmap^ input);
	
	private:
		Mat convertBitmapToMat(WriteableBitmap^ input);
		byte* GetPointerToPixelBuffer(IBuffer^ pixelBuffer);
		WriteableBitmap^ convertMatToBitmap(Mat input);

		Mat processFilter(Mat input);
	};
}

#endif // BARCODEFILTER_H

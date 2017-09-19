#include "pch.h"
#include "barcodeFilter.h"

using namespace Clefable;
using namespace Platform;

barcodeFilter::barcodeFilter()
{


};

// =================================================================================
// input, output and process
// =================================================================================

WriteableBitmap^ barcodeFilter::processFilterBitmap(WriteableBitmap^ input)
{
	Mat matInput = convertBitmapToMat(input);
	Mat matFilter = processFilter(matInput);
	return convertMatToBitmap(matFilter);
}

WriteableBitmap^ barcodeFilter::processFilterTest(WriteableBitmap^ input)
{
	return input;
}

Mat barcodeFilter::processFilter(Mat input)
{
	//Mat matGray = Mat(input.rows, input.cols, CV_8UC4);
	//cvtColor(input, matGray, CV_BGR2BGRA);
	
	return input;
	//return matGray;
}

// =================================================================================
// format conversion
// =================================================================================

Mat barcodeFilter::convertBitmapToMat(WriteableBitmap^ input)
{
	auto image = GetPointerToPixelBuffer(input->PixelBuffer);

	return Mat(input->PixelHeight, input->PixelWidth, CV_8UC4, image);
}

byte* barcodeFilter::GetPointerToPixelBuffer(IBuffer^ pixelBuffer)
{
	ComPtr<IBufferByteAccess> bufferByteAccess;

	reinterpret_cast<IInspectable*>(pixelBuffer)->QueryInterface(IID_PPV_ARGS(&bufferByteAccess));

	byte* pixels = nullptr;
	bufferByteAccess->Buffer(&pixels);

	return pixels;
}

WriteableBitmap^ barcodeFilter::convertMatToBitmap(Mat input)
{
	WriteableBitmap^ bitmap = ref new WriteableBitmap(input.cols, input.rows);

	IBuffer^ buffer = bitmap->PixelBuffer;
	unsigned char* dstPixels = nullptr;

	ComPtr<IBufferByteAccess> pBufferByteAccess;
	ComPtr<IInspectable> pBuffer((IInspectable*)buffer);
	pBuffer.As(&pBufferByteAccess);

	HRESULT get_bytes = pBufferByteAccess->Buffer(&dstPixels);
	if (get_bytes == S_OK) {
		memcpy(dstPixels, input.data, input.step.buf[1] * input.cols*input.rows);
		return bitmap;
	}
}
;

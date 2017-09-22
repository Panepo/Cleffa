//
// MainPage.xaml.cpp
// MainPage 類別的實作。
//

#include "pch.h"
#include "MainPage.xaml.h"

using namespace testClefableCP;

using namespace concurrency;
using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;

using namespace Windows::Storage;
using namespace Windows::Storage::Pickers;
using namespace Windows::Storage::Streams;

using namespace Windows::Graphics::Imaging;

//空白頁項目範本收錄在 http://go.microsoft.com/fwlink/?LinkId=402352clcid=0x409

MainPage::MainPage()
{
	InitializeComponent();
}

void testClefableCP::MainPage::inputButton_Click(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	greetingOutput->Text = "";

	FileOpenPicker^ openPicker = ref new FileOpenPicker();
	openPicker->ViewMode = PickerViewMode::Thumbnail;
	openPicker->SuggestedStartLocation = PickerLocationId::PicturesLibrary;
	openPicker->FileTypeFilter->Append(".jpg");
	openPicker->FileTypeFilter->Append(".jpeg");
	openPicker->FileTypeFilter->Append(".png");
	openPicker->FileTypeFilter->Append(".bmp");

	create_task(openPicker->PickSingleFileAsync()).then([this](StorageFile^ file)
	{
		if (file)
		{
			greetingOutput->Text = "Picked photo: " + file->Name;
		}
		else
		{
			greetingOutput->Text = "Operation cancelled.";
		}
	});
}
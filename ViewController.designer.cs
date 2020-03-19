// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace ThermodynamicsPlotter
{
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSTextField PathInputField { get; set; }

		[Outlet]
		PdfKit.PdfView PlotPreviewView { get; set; }

		[Action ("OpenFileButtonAction:")]
		partial void OpenFileButtonAction (Foundation.NSObject sender);

		[Action ("RedrawPlotAction:")]
		partial void RedrawPlotAction (Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (PlotPreviewView != null) {
				PlotPreviewView.Dispose ();
				PlotPreviewView = null;
			}

			if (PathInputField != null) {
				PathInputField.Dispose ();
				PathInputField = null;
			}
		}
	}
}

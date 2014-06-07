
// This file has been generated by the GUI designer. Do not modify.
namespace MonoDevelop.PackageManagement
{
	public partial class LicenseAcceptanceDialog2
	{
		private global::Gtk.HBox subTitleHBoxForSinglePackage;
		
		private global::Gtk.Label subTitleLabelForSinglePackage;
		
		private global::Gtk.HBox subTitleHBoxForMultiplePackages;
		
		private global::Gtk.Label subTitleLabelForMultiplePackages;
		
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		
		private global::Gtk.VBox packagesVBox;
		
		private global::Gtk.HBox bottomMessageHBox;
		
		private global::Gtk.Label mainMessageLabel;
		
		private global::Gtk.Button buttonCancel;
		
		private global::Gtk.Button buttonOk;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MonoDevelop.PackageManagement.LicenseAcceptanceDialog2
			this.Name = "MonoDevelop.PackageManagement.LicenseAcceptanceDialog2";
			this.Title = global::Mono.Unix.Catalog.GetString ("License Agreements");
			this.WindowPosition = ((global::Gtk.WindowPosition)(1));
			this.Modal = true;
			// Internal child MonoDevelop.PackageManagement.LicenseAcceptanceDialog2.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "mainVBox";
			w1.BorderWidth = ((uint)(2));
			// Container child mainVBox.Gtk.Box+BoxChild
			this.subTitleHBoxForSinglePackage = new global::Gtk.HBox ();
			this.subTitleHBoxForSinglePackage.Name = "subTitleHBoxForSinglePackage";
			this.subTitleHBoxForSinglePackage.Spacing = 6;
			// Container child subTitleHBoxForSinglePackage.Gtk.Box+BoxChild
			this.subTitleLabelForSinglePackage = new global::Gtk.Label ();
			this.subTitleLabelForSinglePackage.Name = "subTitleLabelForSinglePackage";
			this.subTitleLabelForSinglePackage.LabelProp = global::Mono.Unix.Catalog.GetString ("The following package requires a click-to-accept license:");
			this.subTitleHBoxForSinglePackage.Add (this.subTitleLabelForSinglePackage);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.subTitleHBoxForSinglePackage [this.subTitleLabelForSinglePackage]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			w1.Add (this.subTitleHBoxForSinglePackage);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(w1 [this.subTitleHBoxForSinglePackage]));
			w3.Position = 0;
			w3.Expand = false;
			w3.Fill = false;
			// Container child mainVBox.Gtk.Box+BoxChild
			this.subTitleHBoxForMultiplePackages = new global::Gtk.HBox ();
			this.subTitleHBoxForMultiplePackages.Name = "subTitleHBoxForMultiplePackages";
			this.subTitleHBoxForMultiplePackages.Spacing = 6;
			// Container child subTitleHBoxForMultiplePackages.Gtk.Box+BoxChild
			this.subTitleLabelForMultiplePackages = new global::Gtk.Label ();
			this.subTitleLabelForMultiplePackages.Name = "subTitleLabelForMultiplePackages";
			this.subTitleLabelForMultiplePackages.LabelProp = global::Mono.Unix.Catalog.GetString ("The following packages require a click-to-accept license:");
			this.subTitleHBoxForMultiplePackages.Add (this.subTitleLabelForMultiplePackages);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(this.subTitleHBoxForMultiplePackages [this.subTitleLabelForMultiplePackages]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			w1.Add (this.subTitleHBoxForMultiplePackages);
			global::Gtk.Box.BoxChild w5 = ((global::Gtk.Box.BoxChild)(w1 [this.subTitleHBoxForMultiplePackages]));
			w5.Position = 1;
			w5.Expand = false;
			w5.Fill = false;
			// Container child mainVBox.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			global::Gtk.Viewport w6 = new global::Gtk.Viewport ();
			w6.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.packagesVBox = new global::Gtk.VBox ();
			this.packagesVBox.Name = "packagesVBox";
			this.packagesVBox.Spacing = 6;
			w6.Add (this.packagesVBox);
			this.GtkScrolledWindow.Add (w6);
			w1.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(w1 [this.GtkScrolledWindow]));
			w9.Position = 2;
			w9.Padding = ((uint)(3));
			// Container child mainVBox.Gtk.Box+BoxChild
			this.bottomMessageHBox = new global::Gtk.HBox ();
			this.bottomMessageHBox.Name = "bottomMessageHBox";
			this.bottomMessageHBox.Spacing = 6;
			// Container child bottomMessageHBox.Gtk.Box+BoxChild
			this.mainMessageLabel = new global::Gtk.Label ();
			this.mainMessageLabel.Name = "mainMessageLabel";
			this.mainMessageLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("By clicking \"OK\" you agree to the license terms for the packages listed above.\nIf" +
			" you do not agree to the license terms click \"Cancel\".");
			this.bottomMessageHBox.Add (this.mainMessageLabel);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.bottomMessageHBox [this.mainMessageLabel]));
			w10.Position = 0;
			w10.Expand = false;
			w10.Fill = false;
			w1.Add (this.bottomMessageHBox);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(w1 [this.bottomMessageHBox]));
			w11.Position = 3;
			w11.Expand = false;
			w11.Fill = false;
			// Internal child MonoDevelop.PackageManagement.LicenseAcceptanceDialog2.ActionArea
			global::Gtk.HButtonBox w12 = this.ActionArea;
			w12.Name = "mainButtonArea";
			w12.Spacing = 10;
			w12.BorderWidth = ((uint)(5));
			w12.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child mainButtonArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w13 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w12 [this.buttonCancel]));
			w13.Expand = false;
			w13.Fill = false;
			// Container child mainButtonArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w14 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w12 [this.buttonOk]));
			w14.Position = 1;
			w14.Expand = false;
			w14.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 447;
			this.DefaultHeight = 300;
			this.Show ();
		}
	}
}

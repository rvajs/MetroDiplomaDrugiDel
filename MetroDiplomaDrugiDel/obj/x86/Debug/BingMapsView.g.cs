﻿

#pragma checksum "F:\Development\Net\Projects\Windows8\MetroDiplomaDrugiDel\MetroDiplomaDrugiDel\BingMapsView.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5A2CC2291A568BDECEB7EAA8C22B4024"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MetroDiplomaDrugiDel
{
    partial class BingMapsView : global::Windows.UI.Xaml.Controls.UserControl, global::Windows.UI.Xaml.Markup.IComponentConnector
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 4.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
 
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 1:
                #line 29 "..\..\..\BingMapsView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.ClearMapBtn_Click;
                 #line default
                 #line hidden
                break;
            case 2:
                #line 51 "..\..\..\BingMapsView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.RouteBtn_Click;
                 #line default
                 #line hidden
                break;
            case 3:
                #line 56 "..\..\..\BingMapsView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.Selector)(target)).SelectionChanged += this.GeocodeResultSelected;
                 #line default
                 #line hidden
                break;
            case 4:
                #line 38 "..\..\..\BingMapsView.xaml"
                ((global::Windows.UI.Xaml.Controls.Primitives.ButtonBase)(target)).Click += this.GeocodeBtn_Click;
                 #line default
                 #line hidden
                break;
            }
            this._contentLoaded = true;
        }
    }
}



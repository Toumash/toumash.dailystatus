﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DailyStatus.UI.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.4.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("money")]
        public string display_mainkpi_type {
            get {
                return ((string)(this["display_mainkpi_type"]));
            }
            set {
                this["display_mainkpi_type"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public long api_workspaceid {
            get {
                return ((long)(this["api_workspaceid"]));
            }
            set {
                this["api_workspaceid"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public decimal hour_rate {
            get {
                return ((decimal)(this["hour_rate"]));
            }
            set {
                this["hour_rate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("10")]
        public int start_hour {
            get {
                return ((int)(this["start_hour"]));
            }
            set {
                this["start_hour"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public int hours_a_day {
            get {
                return ((int)(this["hours_a_day"]));
            }
            set {
                this["hours_a_day"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1/01/2019,01/06/2019,04/21/2019,04/22/2019,05/01/2019,05/03/2019,06/09/2019,06/20" +
            "/2019,08/15/2019,11/01/2019,11/11/2019,12/25/2019,12/26/2019")]
        public string holidays {
            get {
                return ((string)(this["holidays"]));
            }
            set {
                this["holidays"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2019-09-01")]
        public global::System.DateTime sum_since {
            get {
                return ((global::System.DateTime)(this["sum_since"]));
            }
            set {
                this["sum_since"] = value;
            }
        }
    }
}

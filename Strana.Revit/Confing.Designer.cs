﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Strana.Revit.HoleTask {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.7.0.0")]
    internal sealed partial class Confing : global::System.Configuration.ApplicationSettingsBase {
        
        private static Confing defaultInstance = ((Confing)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Confing())));
        
        public static Confing Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\sb-sharegp\\StranaDev_FamilyManager\\430_Задания на отверстия\\(Отв_Задание)_Перек" +
            "рытия_Прямоугольное.rfa")]
        public string floorHoleTaskPath {
            get {
                return ((string)(this["floorHoleTaskPath"]));
            }
            set {
                this["floorHoleTaskPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("\\\\sb-sharegp\\StranaDev_FamilyManager\\430_Задания на отверстия\\(Отв_Задание)_Стены" +
            "_Прямоугольное.rfa")]
        public string wallHoleTaskPath {
            get {
                return ((string)(this["wallHoleTaskPath"]));
            }
            set {
                this["wallHoleTaskPath"] = value;
            }
        }
    }
}

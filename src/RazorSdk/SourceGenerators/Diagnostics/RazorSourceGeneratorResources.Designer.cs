//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.NET.Sdk.Razor.SourceGenerators.Diagnostics {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class RazorSourceGeneratorResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal RazorSourceGeneratorResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.NET.Sdk.Razor.SourceGenerators.Diagnostics.RazorSourceGeneratorResource" +
                            "s", typeof(RazorSourceGeneratorResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GeneratedOutputFullPath not specified for additional file: {0}.
        /// </summary>
        public static string GeneratedOutputFullPathNotProvidedMessage {
            get {
                return ResourceManager.GetString("GeneratedOutputFullPathNotProvidedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to GeneratedOutputFullPath not defined.
        /// </summary>
        public static string GeneratedOutputFullPathNotProvidedTitle {
            get {
                return ResourceManager.GetString("GeneratedOutputFullPathNotProvidedTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid value &apos;{0}&apos;&apos; for RazorLangVersion. Valid values include &apos;Latest&apos; or a valid version in range 1.0 to 5.0..
        /// </summary>
        public static string InvalidRazorLangMessage {
            get {
                return ResourceManager.GetString("InvalidRazorLangMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid RazorLangVersion.
        /// </summary>
        public static string InvalidRazorLangTitle {
            get {
                return ResourceManager.GetString("InvalidRazorLangTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Recomputing tag helpers from target MetadataReference: {0}..
        /// </summary>
        public static string RecomputingTagHelpersMessage {
            get {
                return ResourceManager.GetString("RecomputingTagHelpersMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Recomputing tag helpers.
        /// </summary>
        public static string RecomputingTagHelpersTitle {
            get {
                return ResourceManager.GetString("RecomputingTagHelpersTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TargetPath not specified for additional file: {0}.
        /// </summary>
        public static string TargetPathNotProvidedMessage {
            get {
                return ResourceManager.GetString("TargetPathNotProvidedMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to TargetPath not defined.
        /// </summary>
        public static string TargetPathNotProvidedTitle {
            get {
                return ResourceManager.GetString("TargetPathNotProvidedTitle", resourceCulture);
            }
        }
    }
}

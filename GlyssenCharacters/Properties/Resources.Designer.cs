﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GlyssenCharacters.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GlyssenCharacters.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #Character ID	Max Speakers	Gender	Age	Status	Comment	Reference	FCBH Character
        ///2 other disciples	2	Male	Adult		Not Peter (Simon), Thomas, Nathaniel, James, or John	JHN 21:3 &amp; JHN 21:5
        ///250 Israelite leaders	250	Male	Adult			NUM 16:3
        ///a Jew	1	Male	Adult			JHN 3:26
        ///Aaron	1	Male	Adult			EXO 5:1 &lt;-(18 more)-&gt; NUM 16:22
        ///Abednego	1	Male	Adult		original Hebrew name: Azariah	DAN 3:16 &lt;-(1 more)-&gt; DAN 3:18
        ///Abigail	1	Female	Adult			1SA 25:19 &lt;-(8 more)-&gt; 1SA 25:41
        ///Abijah, king of Judah	1	Male	Adult			2CH 13:4 &lt;-( [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CharacterDetail {
            get {
                return ResourceManager.GetString("CharacterDetail", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Control File Version	163
        ///#	C	V	Character ID	Delivery	Alias	Quote Type	Default Character	Parallel Passage
        ///# DEU Almost the whole book is by Moses -- In some Bibles, first level quotes are actually 2nd level -- see DEU 1.5
        ///GEN	1	3	God		God (Yahweh)	Normal		
        ///GEN	1	5	narrator-GEN			Quotation		
        ///#Languages which do not allow indirect speech may have God speak to assign the names of things in vv. 5, 8, and 10.
        ///GEN	1	5	God			Indirect		
        ///GEN	1	6	God		God (Yahweh)	Normal		
        ///GEN	1	8	narrator-GEN			Quotation		
        ///G [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CharacterVerseData {
            get {
                return ResourceManager.GetString("CharacterVerseData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;
        ///&lt;!--The entries in this file are somewhat similar to the entries in CharacterVerse.txt marked as &quot;Implicit&quot;. The distinction is that
        ///implicit speech is normally found in the context of a historical narrative, introduced explicitly by the narrator and could/should
        ///be in quotes, but might not be for practical/stylistic reasons. Whereas, narrator overrides are books or passages where the author
        ///is using first-person speech to give a historical account, relate a poem, [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NarratorOverrides {
            get {
                return ResourceManager.GetString("NarratorOverrides", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;RelatedCharacterSets&gt;
        ///	&lt;RelatedCharacters RelationshipType=&quot;SameCharacterWithMultipleAges&quot;&gt;
        ///		&lt;CharacterId&gt;Barzillai&lt;/CharacterId&gt;
        ///		&lt;CharacterId&gt;Barzillai (old)&lt;/CharacterId&gt;
        ///	&lt;/RelatedCharacters&gt;
        ///	&lt;RelatedCharacters RelationshipType=&quot;SameCharacterWithMultipleAges&quot;&gt;
        ///		&lt;CharacterId&gt;David&lt;/CharacterId&gt;
        ///		&lt;CharacterId&gt;David (old)&lt;/CharacterId&gt;
        ///	&lt;/RelatedCharacters&gt;
        ///	&lt;RelatedCharacters RelationshipType=&quot;SameCharacterWithMultipleAges&quot;&gt;
        ///		&lt;CharacterId&gt;Elisha&lt;/CharacterId&gt;
        ///		&lt;CharacterId&gt;Elisha (old)&lt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string RelatedCharacters {
            get {
                return ResourceManager.GetString("RelatedCharacters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-16&quot;?&gt;
        ///&lt;StyleToCharacterMappings&gt;
        ///    &lt;StyleMapping sf=&quot;wj&quot; character=&quot;Jesus&quot;/&gt;
        ///    &lt;StyleMapping sf=&quot;qt&quot; character=&quot;scripture&quot;/&gt;
        ///    &lt;StyleMapping sf=&quot;d&quot; paragraph=&quot;true&quot; character=&quot;Narrator&quot;/&gt;
        ///    &lt;StyleMapping sf=&quot;qa&quot; paragraph=&quot;true&quot; character=&quot;BookOrChapter&quot;/&gt;
        ///&lt;/StyleToCharacterMappings&gt;
        ///.
        /// </summary>
        internal static string StyleToCharacterMappings {
            get {
                return ResourceManager.GetString("StyleToCharacterMappings", resourceCulture);
            }
        }
    }
}

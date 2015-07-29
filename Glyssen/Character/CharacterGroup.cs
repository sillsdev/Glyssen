using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Glyssen.Utilities;

namespace Glyssen.Character
{
	public class CharacterGroup
	{
		private List<CharacterGroupAttribute> m_requiredAttributes; 
		private bool m_isActorAssigned;
		private VoiceActor.VoiceActor m_actorAssigned;

		//For Serialization
		public CharacterGroup()
		{
			CharacterIds = new CharacterIdHashSet();
			GenderAttributes = new CharacterGroupAttributeSet();
			AgeAttributes = new CharacterGroupAttributeSet();
			m_requiredAttributes = new List<CharacterGroupAttribute>();
		}

		public CharacterGroup(int groupNumber) : this()
		{
			GroupNumber = groupNumber;
		}

		public void AssignVoiceActor(VoiceActor.VoiceActor actor)
		{
			if (actor == null)
			{
				return;
			}

			m_isActorAssigned = true;
			m_actorAssigned = actor;
		}

		public void PopulateRequiredAttributes()
		{
			m_requiredAttributes.Clear();
			m_requiredAttributes.InsertRange(m_requiredAttributes.Count, GenderAttributes.OrderByDescending(t => t.Count));
			m_requiredAttributes.InsertRange(m_requiredAttributes.Count, AgeAttributes.OrderByDescending(t => t.Count));			
		}

		public void RemoveVoiceActor()
		{
			m_isActorAssigned = false;
			m_actorAssigned = null;
		}

		[XmlElement]
		public int GroupNumber { get; set; }

		[XmlArray("CharacterIds")]
		[XmlArrayItem("CharacterId")]
		public CharacterIdHashSet CharacterIds { get; set; }

		[XmlArray("Genders")]
		[XmlArrayItem("Gender")]
		[Browsable(false)]
		public CharacterGroupAttributeSet GenderAttributes { get; set; }

		[XmlArray("Ages")]
		[XmlArrayItem("Age")]
		[Browsable(false)]
		public CharacterGroupAttributeSet AgeAttributes { get; set; }

		[XmlIgnore]
		public string RequiredAttributesString
		{
			get { return m_requiredAttributes.Count == 0 ? null : string.Join("; ", m_requiredAttributes.Select(t => t.Name + " [" + t.Count + "]")); }
		}

		[XmlElement]
		public double EstimatedHours { get; set; }

		[XmlIgnore]
		[Browsable(false)]
		public bool IsVoiceActorAssigned 
		{
			get { return m_isActorAssigned; }
		}

		[XmlIgnore]
		[Browsable(false)]
		public VoiceActor.VoiceActor VoiceActorAssigned
		{
			get { return m_actorAssigned; }
		}

		[XmlElement]
		[Browsable(false)]
		public int VoiceActorAssignedId
		{
			get { return m_actorAssigned == null ? -1 : m_actorAssigned.Id; }
			set
			{
				m_actorAssigned = new VoiceActor.VoiceActor();
				m_actorAssigned.Id = value;
				m_isActorAssigned = true;
				if (value < 0)
				{
					m_isActorAssigned = false;
				}
			}
		}

		[XmlIgnore]
		public string VoiceActorAssignedName
		{
			get { return m_isActorAssigned ? m_actorAssigned.Name : ""; }
		}
	}

	#region CharacterGroupAttribute Definition

	public class CharacterGroupAttributeSet : HashSet<CharacterGroupAttribute>
	{
		private readonly Dictionary<string, CharacterGroupAttribute> m_entryNameToDataEntry;

		public CharacterGroupAttributeSet()
		{
			m_entryNameToDataEntry = new Dictionary<string, CharacterGroupAttribute>();
		}

		public void Add(string entryName)
		{
			if (!m_entryNameToDataEntry.ContainsKey(entryName))
			{
				var newEntry = new CharacterGroupAttribute(entryName);
				Add(newEntry);
				m_entryNameToDataEntry.Add(entryName, newEntry);
			}

			m_entryNameToDataEntry[entryName].Count++;
		}
	}

	public class CharacterGroupAttribute
	{
		public CharacterGroupAttribute()
		{

		}

		public CharacterGroupAttribute(string name, int count = 0)
		{
			Name = name;
			Count = count;
		}

		[XmlText]
		public string Name { get; set; }
		[XmlAttribute("Count")]
		public int Count { get; set; }
	}

	#endregion

	#region CharacterIdHashSet Definition
		public class CharacterIdHashSet : HashSet<string>
		{
			public override string ToString()
			{
				return string.Join("; ", this.Select(CharacterVerseData.GetCharacterNameForUi));
			}

			public List<string> ToList()
			{
				return this.Select(CharacterVerseData.GetCharacterNameForUi).ToList();
			}
		}
	#endregion
}

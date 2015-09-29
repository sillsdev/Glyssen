using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Glyssen.Character;
using SIL.Extensions;
using SIL.ObjectModel;

namespace Glyssen.VoiceActor
{
	/// <summary>
	/// This class is basically a hack.
	/// It allows us to sort VoiceActors so the assigned ones go to the bottom.
	/// </summary>
	public class VoiceActorSortableBindingList : SortableBindingList<VoiceActor>
	{
		public VoiceActorSortableBindingList(IList<VoiceActor> list)
			: base(list)
		{
		}

		public IEnumerable<CharacterGroup> CharacterGroups { get; set; }

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			base.ApplySortCore(property, direction);
			if (CharacterGroups == null || !CharacterGroups.Any())
				return;

			var assignedActors = CharacterGroups.Where(cg => cg.IsVoiceActorAssigned).Select(cg => cg.VoiceActorId);
			if (!assignedActors.Any())
				return;

			List<VoiceActor> assignedActorsSorted = new List<VoiceActor>();
			foreach (VoiceActor actor in this.ToList())
			{
				if (assignedActors.Contains(actor.Id))
				{
					Remove(actor);
					assignedActorsSorted.Add(actor);
				}
			}

			this.AddRange(assignedActorsSorted);
		}
	}
}

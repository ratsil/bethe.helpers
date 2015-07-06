using System;
using System.Collections.Generic;
using System.Text;

using System.Collections;
using System.Collections.ObjectModel;
using helpers.extensions;

namespace helpers
{
	public class IdNamePair
	{
		public class Collection : Collection<IdNamePair>
		{
			public class ChangedEventArgs : EventArgs
			{
				public enum ChangeType
				{
					Added,
					Removed,
					Replaced,
					Cleared
				}

				public readonly ChangeType enChangeType;
				public readonly int nIndex;
				public readonly IdNamePair cChangedItem = null;
				public readonly IdNamePair cReplacedWith = null;

				public ChangedEventArgs(ChangeType enCT, int nIndex, IdNamePair cItem, IdNamePair cReplacement)
				{
					enChangeType = enCT;
					cChangedItem = cItem;
					cReplacedWith = cReplacement;
					this.nIndex = nIndex;
				}
				public ChangedEventArgs(ChangeType enCT, int nIndex, IdNamePair cItem) : this(enCT, nIndex, cItem, null) { }
				public ChangedEventArgs(ChangeType enCT) : this(enCT, -1, null, null) { }
			}
			public event EventHandler<ChangedEventArgs> Changed;

			protected override void InsertItem(int nIndex, IdNamePair cItem)
			{
				base.InsertItem(nIndex, cItem);

				EventHandler<ChangedEventArgs> eTemp = Changed;
				if (eTemp != null)
					eTemp(this, new ChangedEventArgs(ChangedEventArgs.ChangeType.Added, nIndex, cItem));
			}
			protected override void SetItem(int nIndex, IdNamePair cItem)
			{
				IdNamePair cReplaced = Items[nIndex];
				base.SetItem(nIndex, cItem);

				EventHandler<ChangedEventArgs> eTemp = Changed;
				if (eTemp != null)
					eTemp(this, new ChangedEventArgs(ChangedEventArgs.ChangeType.Replaced, nIndex, cItem, cReplaced));
			}
			protected override void RemoveItem(int nIndex)
			{
				IdNamePair cRemovedItem = Items[nIndex];
				base.RemoveItem(nIndex);

				EventHandler<ChangedEventArgs> eTemp = Changed;
				if (eTemp != null)
					eTemp(this, new ChangedEventArgs(ChangedEventArgs.ChangeType.Removed, nIndex, cRemovedItem));
			}
			protected override void ClearItems()
			{
				base.ClearItems();

				EventHandler<ChangedEventArgs> eTemp = Changed;
				if (eTemp != null)
					eTemp(this, new ChangedEventArgs(ChangedEventArgs.ChangeType.Cleared));
			}
		}
		public long nID;
		public string sName;

		public IdNamePair()
		{
			nID = -1;
			sName = null;
		}
		public IdNamePair(long nID, string sName)
			: this()
		{
			this.nID = nID;
			this.sName = sName;
		}
		public IdNamePair(string sName)
			: this(-1, sName) { }
		public IdNamePair(object oID, object oName) : this(oID.ToID(), oName.ToString()) { }
		public IdNamePair(Hashtable aValues) : this(aValues["id"], aValues["sName"]) { }

		public override int GetHashCode()
		{
			return nID.GetHashCode();
		}
		override public string ToString()
		{
			return sName;
		}
	}
}

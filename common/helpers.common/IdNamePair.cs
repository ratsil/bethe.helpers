using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using helpers.extensions;
using System.Xml.Serialization;

namespace helpers
{
	//[XmlRoot("IdNamePair", Namespace = "helpers")]
	[Serializable]
	public class IdNamePair
	{
		public class Collection : Collection<IdNamePair>   // не юзается нигде
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
			nID = extensions.x.ToID(null);
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
		public IdNamePair(object oID, object oName) 
            : this(oID.ToID(), oName.ToString().FromDB()) { }
		public IdNamePair(Hashtable aValues) 
            : this(aValues["id"], aValues["sName"]) { }

		public override int GetHashCode()
		{
			return nID.GetHashCode();
		}
		override public string ToString()
        {
            string sRetVal = "(" + nID.ToString() + ", NULL)";
            return sName == null ? sRetVal : sRetVal.Replace("NULL", sName);
        }
        public static IdNamePair[] GetArray(object oArray)
        {
            if (null == oArray)
                return null;
            string sArray = oArray.ToString();
            if (sArray.IsNullOrEmpty() || sArray == "{}" || sArray == "{NULL}")
                return null;
            string[] aPair, aRows;
            aRows = sArray.Replace("{\"(", "").Replace(")\"}", "").Split(new string[1] { ")\",\"(" }, StringSplitOptions.RemoveEmptyEntries);
            List<IdNamePair> aRetVal = new List<IdNamePair>();
            for (int nI = 0; nI < aRows.Length; nI++)
            {
                aPair = aRows[nI].Split(',');
                if (aPair.Length < 2)
                    break;
                aRetVal.Add(new IdNamePair(aPair[0].ToID(), aPair[1].Replace("\\\"", "")));
            }
            return aRetVal.ToArray();
        }
    }
}

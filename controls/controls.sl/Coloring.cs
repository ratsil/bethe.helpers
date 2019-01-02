using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Browser;
using System.Linq;

namespace controls.sl
{
	public class Coloring
	{
        public enum DataGridRowColorType
        {
            Normal,
            MouseOver,
            Selected
        }
		public static class Notifications
		{
			public static Brush cTextBoxError = new SolidColorBrush(Color.FromArgb(255, 255, 170, 170));
			public static Brush cTextBoxChanged = new SolidColorBrush(Color.FromArgb(255, 170, 255, 170));
			public static Brush cTextBoxActive = new SolidColorBrush(Color.FromArgb(255, 255, 249, 235));
			public static Brush cTextBoxInactive = new SolidColorBrush(Color.FromArgb(255, 229, 232, 235));
			public static Brush cButtonNormal = new SolidColorBrush(Color.FromArgb(255, 255, 191, 46));
			public static Brush cButtonError = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
			public static Brush cButtonChanged = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
			public static Brush cButtonInactive = new SolidColorBrush(Color.FromArgb(255, 31, 59, 83));
			public static Brush cErrorForeground = new SolidColorBrush(Color.FromArgb(255, 188, 0, 0));
			public static Brush cChangedForeground = new SolidColorBrush(Color.FromArgb(255, 0, 133, 0));
			public static Brush cNormalForeground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            public static Brush cInactiveForeground = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
        }
		public static class Playlist
		{
			public static Brush cRow_CachedBackgr { get { return new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)); } set { } }   // 233, 219, 255));
			public static Brush cRow_UnCachedBackgr = new SolidColorBrush(Color.FromArgb(255, 243, 255, 252));
			public static Brush cRow_OnAirQueuedBackgr = new SolidColorBrush(Color.FromArgb(255, 209, 255, 209));
			public static Brush cRow_OnAirPreparedBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 255, 177));
			public static Brush cRow_OnAirOnAirBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 226, 226));
			public static Brush cRow_PlannedInsertedForegr = new SolidColorBrush(Color.FromArgb(255, 162, 200, 222));   // LightBlue
			public static Brush cRow_PlannedNormalForegr = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
			public static Brush cRow_PlannedAdvBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 226, 209));   //255, 241, 209
			public static Brush cRow_PlannedClipBackgr = new SolidColorBrush(Color.FromArgb(255, 227, 245, 217));
			public static Brush cRow_PlannedDesignBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 252, 219));
			public static Brush cRow_PlannedProgramBackgr = new SolidColorBrush(Color.FromArgb(255, 233, 228, 255));
			public static Brush cRow_PlannedTrailersBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 236, 195));
			public static Brush cRow_PlannedPlugsBackgr = new SolidColorBrush(Color.FromArgb(255, 208, 255, 241));
			public static Brush cRow_PlannedOtherBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
			//public static Brush cLightPink = new SolidColorBrush(Color.FromArgb(255, 255, 205, 255));  
			public static Brush cTypeColumn_AdvertsBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 161, 147));
			public static Brush cTypeColumn_ClipsBackgr = new SolidColorBrush(Color.FromArgb(255, 176, 226, 176));
			public static Brush cTypeColumn_ProgramsBackgr = new SolidColorBrush(Color.FromArgb(255, 179, 194, 232));
			public static Brush cTypeColumn_DesignBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 255, 147));
			public static Brush cTypeColumn_PlugBackgr = new SolidColorBrush(Color.FromArgb(255, 159, 255, 227));
			public static Brush cTypeColumn_InsertedBackgr = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
			public static Brush cTypeColumn_ErrorBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 58, 58));
			public static Brush cRow_PluginNormalBackgr = new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
		}
		public static class Templates
		{
			public static Brush cRow_NormalBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 252, 219));
			public static Brush cRow_FutureBackgr = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
			public static Brush cRow_StopOnThisWeekBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 161, 147));
			public static Brush cRow_ChangedBackgr = new SolidColorBrush(Color.FromArgb(255, 170, 255, 170));
		}
		public static class AssetsList
		{
			public static Brush cRow_ProgramSeriesBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 247, 237));
			public static Brush cRow_ProgramEpisodeBackgr = new SolidColorBrush(Color.FromArgb(255, 244, 241, 255));
			public static Brush cRow_ProgramPartBackgr = new SolidColorBrush(Color.FromArgb(255, 232, 232, 232));
            public static Brush cRow_ProgramParentEmptyBackgr = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            public static Brush cRow_ItemErrorBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 200, 200));
            public static Brush cRow_ItemErrorNoFileBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 200, 225));
        }
		public static class FilesList
		{
			public static Brush cNormalBackgr = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
			public static Brush cNormalForegr = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
			public static Brush cUnusedBackgr = new SolidColorBrush(Color.FromArgb(255, 232, 242, 226)); //LightGreen
			public static Brush cUnusedForegr = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
		}
		public static class SMS
		{
			public static Brush cRefreshBtnPressed = new SolidColorBrush(Color.FromArgb(255, 255, 157, 255));  // Pink
		}
		public static class Prompter
		{
			public static Brush cPreparedOnScreenBackgr = new SolidColorBrush(Color.FromArgb(255, 55, 55, 88));
			public static Brush cPreparedOnScreenForegr = new SolidColorBrush(Color.FromArgb(255, 255, 255, 111)); //Yellow
			public static Brush cPreparedOffScreenBackgr = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
			public static Brush cPreparedOffScreenForegr = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150)); //LightGreen
			public static Brush cPreparedPauseOnBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
			public static Brush cPreparedPauseOffBackgr = new SolidColorBrush(Color.FromArgb(255, 39, 255, 0));
		}
		public static class SCR
		{
			public static class Timer
			{
				public static Brush cTotalRemainNormalForegr = new SolidColorBrush(Color.FromArgb(255, 94, 255, 82));
				public static Brush cTotalRemainPreWarningForegr = new SolidColorBrush(Color.FromArgb(255, 255, 228, 82));
				public static Brush cTotalRemainWarningForegr = new SolidColorBrush(Color.FromArgb(255, 255, 82, 82));   //Red
				public static Brush cStartBtnTextForegr = new SolidColorBrush(Color.FromArgb(255, 0, 124, 0));
				public static Brush cStopBtnTextForegr = new SolidColorBrush(Color.FromArgb(255, 216, 0, 0));
				public static Brush cAdvTimerWarningForegr = new SolidColorBrush(Color.FromArgb(255, 255, 157, 255));
				public static Brush cAdvTimerNormalForegr = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
			}
			public static Brush cPLRow_AdvBlockItemBackgr = new SolidColorBrush(Color.FromArgb(255, 209, 255, 209));
			public static Brush cPLRow_AdvBlockBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 255, 177));
			public static Brush cPLRow_AdvBlockItemStoppedBackgr = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));
			public static Brush cPLRow_AdvBlockStoppedBackgr = new SolidColorBrush(Color.FromArgb(255, 55, 55, 55));
			public static Brush cPLRow_ClipBackgr = new SolidColorBrush(Color.FromArgb(255, 209, 255, 255));
			public static Brush cPLRow_OthersBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 241, 209));
			public static Brush cPLRow_StoppedForegr = new SolidColorBrush(Color.FromArgb(255, 162, 200, 222));  // LightBlue
			public static Brush cPLRow_NormalForegr = new SolidColorBrush(Color.FromArgb(255, 42, 67, 78));    // DarkBlue
			public static Brush cClipsRow_NormalForegr = new SolidColorBrush(Color.FromArgb(255, 51, 81, 102));  // Blue
			public static Brush cClipsRow_BlockedForegr = new SolidColorBrush(Color.FromArgb(255, 153, 0, 56));   // Red
            public static Brush cClipsRow_NormalForegrNotCached = new SolidColorBrush(Color.FromArgb(255, 154, 183, 203));  // L Blue
            public static Brush cClipsRow_BlockedForegrNotCached = new SolidColorBrush(Color.FromArgb(255, 255, 179, 179));   // LightRed
            public static Brush cUserPlaques_FirstColorBackgr = new SolidColorBrush(Color.FromArgb(255, 255, 241, 209));
			public static Brush cUserPlaques_SecondColorBackgr = new SolidColorBrush(Color.FromArgb(255, 209, 255, 209));
			public static Brush cRefreshBtnPressed = new SolidColorBrush(Color.FromArgb(255, 255, 157, 255));
			public static Brush cRefreshBtnNormal = Timer.cAdvTimerNormalForegr;
            public static class Cached
            {
                public static SolidColorBrush cClipsBackgr = new SolidColorBrush(FromHEX("B9DEEB"));
                public static SolidColorBrush cBlockBackgr = new SolidColorBrush(FromHEX("FCFFC4"));
                public static SolidColorBrush cCachedBackgr = new SolidColorBrush(FromHEX("A3FFAB"));
                public static SolidColorBrush cCachedBackgrGray = new SolidColorBrush(FromHEX("#d1d1d1"));
                public static SolidColorBrush cNotCachedBackgr = new SolidColorBrush(FromHEX("FFA3A3"));
                public static SolidColorBrush cInQueueBackgr = new SolidColorBrush(FromHEX("FFD9A3"));
                public static SolidColorBrush cCachingBackgr = new SolidColorBrush(FromHEX("F6A3FF"));
            }
        }
        private static Color FromHEX(string sHex)
        {
            sHex = sHex.Replace("#", "");
            byte nA = 255;
            int nI = 0;
            if (sHex.Length==8)
            {
                nA = byte.Parse(sHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                nI += 2;
            }
            byte nR = byte.Parse(sHex.Substring(nI, 2), System.Globalization.NumberStyles.HexNumber);
            nI += 2;
            byte nG = byte.Parse(sHex.Substring(nI, 2), System.Globalization.NumberStyles.HexNumber);
            nI += 2;
            byte nB = byte.Parse(sHex.Substring(nI, 2), System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb(nA, nR, nG, nB);
        }
        public static SolidColorBrush ModifyBrushByType(SolidColorBrush cBackgroundBrush, DataGridRowColorType enColorType)
        {
            switch (enColorType)
            {
                case DataGridRowColorType.Normal:
                default:
                    return cBackgroundBrush;
                case DataGridRowColorType.MouseOver:
                    return new SolidColorBrush(Color.FromArgb(cBackgroundBrush.Color.A, ChannelPercent(cBackgroundBrush.Color.R, 10), ChannelPercent(cBackgroundBrush.Color.G, 10), ChannelPercent(cBackgroundBrush.Color.B, 10)));
                case DataGridRowColorType.Selected:
                    return new SolidColorBrush(Color.FromArgb(cBackgroundBrush.Color.A, ChannelPercent(cBackgroundBrush.Color.R, 30), ChannelPercent(cBackgroundBrush.Color.G, 30), ChannelPercent(cBackgroundBrush.Color.B, 30)));
            }
        }
        private static byte ChannelPercent(byte nChannel, byte nChangePercent)
        {
            int nCh = nChannel * nChangePercent / 100;
            nCh = nCh < 15 ? 15 : nCh;
            return (byte)(nChannel > 30 ? nChannel - nCh : nChannel + nCh);
        }
    }
}

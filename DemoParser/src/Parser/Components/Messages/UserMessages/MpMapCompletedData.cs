using System;
using System.Collections.Generic;
using System.Xml.Linq;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	// portal 2 specific
	public class MpMapCompletedData : UserMessage {

		public const int MaxPortal2CoopBranches = 6;
		public const int MaxPortal2CoopLevelsPerBranch = 16;

		public List<MapCompletedInfo> Info;


		public MpMapCompletedData(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			Info = new List<MapCompletedInfo>();
			Span<byte> bytes = stackalloc byte[2 * MaxPortal2CoopBranches * MaxPortal2CoopLevelsPerBranch / 8];
			bsr.ReadToSpan(bytes);
			int current = 0;
			int mask = 0x01;
			for (int player = 0; player < 2; player++) {
				for (int branch = 0; branch < MaxPortal2CoopBranches; branch++) {
					for (int level = 0; level < MaxPortal2CoopLevelsPerBranch; level++) {
						if ((bytes[current] & mask) != 0)
							Info.Add(new MapCompletedInfo {Player = player, Branch = branch, Level = level});
						mask <<= 1;
						if (mask >= 0x0100) {
							current++;
							mask = 0x01;
						}
					}
				}
			}
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			if (Info.Count == 0) {
				pw.Append("none");
			} else {
				pw.Append("maps set as completed: (1-indexed)");
				pw.FutureIndent++;
				for (var i = 0; i < Info.Count; i++) {
					pw.AppendLine();
					pw.Append($"player: {Info[i].Player+1}, branch: {Info[i].Branch+1}, level: {Info[i].Level+1}");
				}
				pw.FutureIndent--;
			}
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("MPMapCompletedData");
			//TODO: this
			parent.Add(thisElement);
		}


		public struct MapCompletedInfo {
			public int Player;
			public int Branch;
			public int Level;
		}
	}
}

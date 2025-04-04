using System.Numerics;
using System.Xml.Linq;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;

namespace DemoParser.Parser.Components.Messages.UserMessages {

	public class PaintWorld : UserMessage {

		public PaintType PaintType;
		public EHandle EHandle;
		public float UnkHf1, UnkHf2;
		public Vector3 Center;
		public Vector3[] Positions;


		public PaintWorld(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			PaintType = (PaintType)bsr.ReadByte();
			EHandle = bsr.ReadEHandle();
			UnkHf1 = bsr.ReadFloat();
			UnkHf2 = bsr.ReadFloat();
			byte len = bsr.ReadByte();
			bsr.ReadVector3(out Center);
			if (bsr.BitsRemaining % 48 != 0) {
				DemoRef.LogError($"{GetType().Name}: wrong number of bits left, expected a multiple of 48 but got {bsr.BitsRemaining}");
				bsr.SetOverflow();
				return;
			}
			Positions = new Vector3[len];
			for (int i = 0; i < len; i++)
				Positions[i] = Center + new Vector3(bsr.ReadSShort(), bsr.ReadSShort(), bsr.ReadSShort());
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"paint type: {PaintType.ToString()}");
			pw.AppendLine($"handle: {EHandle}");
			pw.AppendLine($"unknown floats: {UnkHf1}, {UnkHf2}");
			pw.AppendLine($"center: {Center}");
			pw.Append($"all {Positions.Length} positions: ");
			pw.FutureIndent++;
			foreach (Vector3 pos in Positions) {
				pw.AppendLine();
				pw.Append(pos.ToString());
			}
			pw.FutureIndent--;
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("PaintWorld");
			//TODO: this
			parent.Add(thisElement);
		}
	}


	public enum PaintType : byte {
		JumpPaint = 0,
		SpeedPaintOther, // has same texture as speed but different value
		SpeedPaint,
		PortalPaint,
		ClearPaint
	}
}

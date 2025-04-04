using DemoParser.Parser.Components.Abstract;
using DemoParser.Utils;
using DemoParser.Utils.BitStreams;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DemoParser.Parser.Components.Messages {

	public class NetSignOnState : DemoMessage {

		public SignOnState SignOnState;
		public int SpawnCount;
		// demo protocol 4 only
		public uint? NumServerPlayers;
		public byte[]? PlayerNetworkIds;
		public string? MapName;


		public NetSignOnState(SourceDemo? demoRef, byte value) : base(demoRef, value) {}


		protected override void Parse(ref BitStreamReader bsr) {
			SignOnState = (SignOnState)bsr.ReadByte();
			SpawnCount = bsr.ReadSInt();
			if (DemoInfo.NewDemoProtocol) {
				NumServerPlayers = bsr.ReadUInt();
				int length = (int)bsr.ReadUInt();
				if (length > 0)
					PlayerNetworkIds = bsr.ReadBytes(length);
				length = (int)bsr.ReadUInt();
				if (length > 0) // the string still seams to be null terminated (sometimes?)
					MapName = bsr.ReadStringOfLength(length).Split(new char[]{'\0'}, 2)[0];
			}
			if (SignOnState == SignOnState.PreSpawn)
				GameState.ClientSoundSequence = 1; // reset sound sequence number after receiving SignOn sounds
		}


		public override void PrettyWrite(IPrettyWriter pw) {
			pw.AppendLine($"sign on state: {SignOnState}");
			pw.Append($"spawn count: {SpawnCount}");
			if (DemoInfo.NewDemoProtocol) {
				pw.Append($"\nnumber of server players: {NumServerPlayers}");
				if (PlayerNetworkIds != null)
					pw.Append($"\nbyte array of length {PlayerNetworkIds.Length}");
				if (MapName != null)
					pw.Append($"\nmap name: {MapName}");
			}
		}

		public override void XMLWrite(XElement parent)
		{
			XElement thisElement = new XElement("NetSignOnState", new XElement("Spawn-Count", SpawnCount), new XElement("SignOnState", SignOnState));
			if (DemoInfo.NewDemoProtocol)
			{
				thisElement.Add(new XElement("Num-Server-Players", NumServerPlayers));
				thisElement.Add(new XElement("Map-Name", MapName));
			}
			parent.Add(thisElement);
		}
	}

	public enum SignOnState {
		None = 0,   // no state yet, about to connect
		Challenge,  // client challenging server, all OOB packets
		Connected,  // client is connected to server, netchans ready
		New,        // just got server info and string tables
		PreSpawn,   // received signon buggers
		Spawn,      // ready to receive entity packets
		Full,       // we are fully connected, first non-delta packet received
		ChangeLevel // server is changing level, please wait
	}
}

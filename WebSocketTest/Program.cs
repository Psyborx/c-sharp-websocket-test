using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperWebSocket;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;

namespace WebSocketTest {
	class Program {

		private static List<WebSocketSession> m_Sessions = new List<WebSocketSession>();
		private static object m_SessionSyncRoot = new object();

		static void Main(string[] args) {

			var config = new ServerConfig {
				Name = "SuperWebSocket",
				Ip = "Any",
				Port = 2012,
				Mode = SocketMode.Tcp,
			};

			Console.WriteLine("Starting WebSocketServer on port " + config.Port + "...");

			var socketServer = new WebSocketServer();
			socketServer.Setup(new RootConfig(), config);

			socketServer.NewMessageReceived += new SessionHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
			socketServer.NewSessionConnected += new SessionHandler<WebSocketSession>(socketServer_NewSessionConnected);
			socketServer.SessionClosed += new SessionHandler<WebSocketSession, CloseReason>(socketServer_SessionClosed);

			if (!socketServer.Start()) {
				Console.WriteLine("Failed to start!");
				Console.ReadKey();
				return;
			}

			Console.WriteLine("The server started successfully, press 'q' key to stop it!");

			while (Console.ReadKey().KeyChar != 'q') {
				Console.WriteLine();
				continue;
			}
			socketServer.Stop();

			Console.WriteLine("\nThe server stopped!");
		}

		static void socketServer_NewMessageReceived(WebSocketSession session, string e) {
			Console.WriteLine("To client: " + e);
			session.Send(session.SessionID + ": " + e);
		}

		static void socketServer_NewSessionConnected(WebSocketSession session) {
			String acceptedOrigin = "http://192.168.31.68";
			//Console.WriteLine(session.RemoteEndPoint.Address.ToString());
			Console.WriteLine(session.Origin);
			if (session.Origin.Equals(acceptedOrigin)) {
				lock (m_SessionSyncRoot)
					m_Sessions.Add (session);

				SendToAll ("System: " + session.SessionID + " connected");
			} else {
				session.Close(CloseReason.SocketError);
			}
		}

		static void socketServer_SessionClosed(WebSocketSession session, CloseReason reason) {
			lock (m_SessionSyncRoot)
				m_Sessions.Remove(session);

			if (reason == CloseReason.ServerShutdown)
				return;

			SendToAll("System: " + session.SessionID + " disconnected");
		}

		static void SendToAll(string message) {
			Console.WriteLine("To All: " + message);
			lock (m_SessionSyncRoot) {
				foreach (var s in m_Sessions) {
					s.Send(message);
				}
			}
		}

	}
}

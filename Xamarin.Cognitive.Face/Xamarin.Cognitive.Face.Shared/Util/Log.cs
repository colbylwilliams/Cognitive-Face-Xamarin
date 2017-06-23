#if DEBUG
using System.Runtime.CompilerServices;
using System.Linq;
#endif

using System;

namespace Xamarin.Cognitive.Face
{
	/// <summary>
	/// Log helper class to make logging details easier.
	/// </summary>
	public static class Log
	{
#if DEBUG

		/// <summary>
		/// Logs a DEBUG <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The debug.</returns>
		/// <param name="caller">Caller.</param>
		/// <param name="methodName">Method name.</param>
		/// <param name="message">Message.</param>
		public static void Debug (object caller, string methodName, string message)
		{
			System.Diagnostics.Debug.WriteLine ($"[{System.DateTime.Now:MM/dd/yyyy h:mm:ss.fff tt}] DEBUG: [{caller.GetType ().Name}] {methodName} : {message}");
		}


		/// <summary>
		/// Logs a DEBUG <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The debug.</returns>
		/// <param name="message">Message.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Debug (string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			sourceFilePath = sourceFilePath.Split ('/').LastOrDefault ();

			System.Diagnostics.Debug.WriteLine ($"[{System.DateTime.Now:MM/dd/yyyy h:mm:ss.fff tt}] DEBUG: [{sourceFilePath}] [{memberName}] [{sourceLineNumber}] : {message}");
		}

#else
		/// <summary>
		/// Logs a DEBUG <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The debug.</returns>
		/// <param name="message">Message.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Debug (string message, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0) { }
#endif

#if DEBUG

		/// <summary>
		/// Logs an INFO <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The info.</returns>
		/// <param name="message">Message.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Info (string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			sourceFilePath = sourceFilePath.Split ('/').LastOrDefault ();

			System.Diagnostics.Debug.WriteLine ($"[{System.DateTime.Now:MM/dd/yyyy h:mm:ss.fff tt}]  Info: [{sourceFilePath}] [{memberName}] [{sourceLineNumber}] : {message}");
			//System.Diagnostics.Trace.WriteLine ($"[{DateTime.Now:MM/dd/yyyy h:mm:ss.fff tt}] [{sourceFilePath}] [{memberName}] [{sourceLineNumber}] : {message}");
		}
#else
		/// <summary>
		/// Logs an INFO <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The info.</returns>
		/// <param name="message">Message.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Info (string message, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0) { }
#endif

#if DEBUG

		/// <summary>
		/// Logs an INFO <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Error (string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			sourceFilePath = sourceFilePath.Split ('/').LastOrDefault ();

			System.Diagnostics.Debug.WriteLine ($"[{System.DateTime.Now:MM/dd/yyyy h:mm:ss.fff tt}] ERROR: [{sourceFilePath}] [{memberName}] [{sourceLineNumber}] : {message}");
			//System.Diagnostics.Trace.WriteLine ($"[{DateTime.Now:MM/dd/yyyy h:mm:ss.fff tt}] [{sourceFilePath}] [{memberName}] [{sourceLineNumber}] : {message}");
		}
#else
		/// <summary>
		/// Logs an ERROR <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="message">Message.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Error (string message, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0) { }
#endif

#if DEBUG

		/// <summary>
		/// Logs an ERROR <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="error">Error.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Error (Exception error, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
		{
			Error (error.Message, memberName, sourceFilePath, sourceLineNumber);
		}
#else
		/// <summary>
		/// Logs an ERROR <c>message</c> using Debug.WriteLine().
		/// </summary>
		/// <returns>The error.</returns>
		/// <param name="error">Error.</param>
		/// <param name="memberName">Member name.</param>
		/// <param name="sourceFilePath">Source file path.</param>
		/// <param name="sourceLineNumber">Source line number.</param>
		public static void Error (Exception error, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0) { }
#endif
	}
}
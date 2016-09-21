using CoreTweet;
using System;

namespace Fonetter {
	static partial class Extensions {
		public static string DetectException(this Exception e) {
			if(!(e is TwitterException)) return e.Message;
			var te = e as TwitterException;
			foreach(var t in te.Errors) {
				switch((ErrorCode)t.Code) {
					case ErrorCode.CannotFavoriteTweetsOfProtectedUsersYouAreNotFollowing:
						return ExceptionCheck.ProtectedUser.ToString();

					case ErrorCode.AlreadyRetweetedThisTweet:
					case ErrorCode.AlreadyFavorited:
						return ExceptionCheck.Already.ToString();

					case ErrorCode.BlockedFromOperatingStatus:
						return ExceptionCheck.Blocked.ToString();

					case ErrorCode.CouldNotFindStatus:
					case ErrorCode.NoStatusFoundWithThatId:
						return ExceptionCheck.Nothing.ToString();

					case ErrorCode.StatusIsDuplicate:
						return ExceptionCheck.Duplicate.ToString();
				}
			}
			return e.Message;
		}
	}
}

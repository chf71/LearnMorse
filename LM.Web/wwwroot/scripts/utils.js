﻿PlayClip = function () {
	try {
		$("#clip")[0].play();
	} catch (error) {
		console.log(error);
	}
}

ResetInputBox = function () {
	MorseInputText = "";
	$("#morseinput").html("");
}

ShowDeckBuilder = function () {
	$("#deckbuilderbox").removeClass("invisible");
}

HideDeckBuilder = function () {
	$("#deckbuilderbox").addClass("invisible");
}

CheckGuestCookie = function () {
	return GetGuestCookieValue() == "";
}

GetGuestCookieValue = function () {
	var cookies = document.cookie.split(';');
	for (i = 0; i < cookies.length; i++) {
		var cookie = cookies[i].trim();
		if (cookie.indexOf("guestCookie") == 0) {
			return cookie.substring("guestCookie".length, cookie.length);
		}
	}
}

SetGuestCookie = function (keyValue) {
	document.cookie = "guestCookie = " + keyValue;
}
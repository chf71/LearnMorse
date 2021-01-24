CorrectIndex = -1;
CardBackImg = "";

SetupMCQuestion = function (cardFaceImg, cardBackImg, correctIndex) {
	CorrectIndex = correctIndex;
	CardBackImg = cardBackImg;

	$("#card").attr("src", cardFaceImg);

	TransitionIn();
}

SetupIntroQuestion = function (cardFaceImg, cardBackImg) {
	CardBackImg = cardBackImg;

	$("#card").attr("src", cardFaceImg);
	$("#card").click(() => { TransitionNext('Intro'); });

	console.log(cardFaceImg);

	TransitionIn();
}


///////////////////////////
// input sound variables //
let context = new AudioContext();
let osci = context.createOscillator();
let gainNode = context.createGain();

gainNode.connect(context.destination);

osci.type = "sine";
osci.frequency.value = 750;
osci.connect(gainNode);
//////////////////////////

InputStartTime = 0;
InputEndTime = 0;
MorseText = "";
MorseInputText = "";

SetupInputQuestion = function (morseText, cardFaceImg, cardBackImg) {
	CardBackImg = cardBackImg;
	MorseText = morseText;

	$("#card").attr("src", cardFaceImg);

	osci = context.createOscillator();
	osci.type = "sine";
	osci.frequency.value = 750;
	osci.connect(gainNode);

	osci.start();
	gainNode.gain.setValueAtTime(
		gainNode.gain.minValue,
		context.currentTime
	);

	TransitionIn();
}

InputMouseDown = function () {
	// begin recording
	InputStartTime = performance.now();

	gainNode.gain.setValueAtTime(
		-0.285,
		context.currentTime
	);

	$("#morseinput").css("background-color", "#4b96e3");
}

InputMouseUp = function () {
	$("#morseinput").css("background-color", "#1b6ec2");

	gainNode.gain.setValueAtTime(
		gainNode.gain.minValue,
		context.currentTime
	);

	// end recording
	InputEndTime = performance.now();

	// calculate difference
	InputTime = InputEndTime - InputStartTime;

	// decide if it's a dit or dah
	if (InputTime >= 500) {
		MorseInputText = MorseInputText.concat("_ ");
	}
	else if (InputTime >= 100 && InputTime < 500) {
		MorseInputText = MorseInputText.concat(". ");
	}

	// set html of input box
	$("#morseinput").html(MorseInputText);

	// check if it matches
	CheckInputAnswer();
}
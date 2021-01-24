TransitionIn = function () {
	DotNet.invokeMethodAsync("LM.Web", "UpdateStateCaller");

	$("#card").fadeIn('slow');
	$("#answerbox").fadeIn('slow');
}

TransitionOut = function () {
	$("#card").fadeOut('slow');
	$("#answerbox").fadeOut('slow');
}

TransitionNext = function (type) {
	$("#card").transition({
		rotateY: "90deg",
		duration: 250,
		complete: () => {
			$("#card").transition({
				rotateY: "0deg",
				duration: 500,
				start: () => {
					PlayClip();
					$("#card").attr("src", CardBackImg);
				},
				complete: () => {
					TransitionOut();
					setTimeout(() => {
						if (type == "Input") {
							ClearInputQuestion();
						}
						else if (type == "MC") {
							ClearMCQuestion();
						}
						else if (type == "Intro") {
							ClearIntroQuestion();
                        }
						DotNet.invokeMethodAsync("LM.Web", "NextQuestionCaller");
					}, 1500);
				}
			});
		}
	});
}
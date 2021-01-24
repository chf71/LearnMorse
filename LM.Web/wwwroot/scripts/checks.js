CheckMCAnswer = function (chosenIndex) {
    if (chosenIndex == CorrectIndex) {
        $("#answer" + chosenIndex).css("background-color", "green");
    }
    else {
        $("#answer" + chosenIndex).css("background-color", "red");
    }

    TransitionNext('MC');
}

CheckInputAnswer = function () {
    if (MorseInputText == MorseText) {
        $("#morseinput").css("background-color", "green");

        context.suspend();

        TransitionNext('Input');
    }
}
ClearMCQuestion = function () {
    CorrectIndex = -1;
    CardBackImg = "";

    $("#answer0").css("background-color", "#1b6ec2");
    $("#answer1").css("background-color", "#1b6ec2");
    $("#answer2").css("background-color", "#1b6ec2");
}

ClearIntroQuestion = function () {
    CardBackImg = "";
    $("#card").unbind("click");
}


ClearInputQuestion = function () {
    InputStartTime = 0;
    InputEndTime = 0;
    MorseText = "";
    CardBackImg = "";
    $("#morseinput").css("background-color", "#1b6ec2");
    $("#morseinput").unbind("mousedown");
    $("#morseinput").unbind("mouseup");
    ResetInputBox();
}
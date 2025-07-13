$(document).ready(function () {

    // Quick menu
    $("#QuickMenuButton").click(function (e) {
        $("#QuickMenuContainer").toggle("slide", { direction: "left" }, 500);

        e.stopPropagation();
    });

    // Mouse click off hides quick menu
    $("html").click(function (e) {
        if ($("#QuickMenuContainer").is(":visible") == true) {
            $("#QuickMenuContainer").toggle("slide", { direction: "left" }, 500);
        }
    });

    // Escape key hides quick menu
    $(document).keyup(function (e) {
        if (e.keyCode == 27) {
            if ($("#QuickMenuContainer").is(":visible") == true) {
                $("#QuickMenuContainer").toggle("slide", { direction: "left" }, 500);
            }
        }
    });

    // Stop event bubbling in quick menu
    $("#QuickMenuContainer").click(function (e) {
        e.stopPropagation();
    });

    // Logo toggling
    $("#GlobalLogo").hover(function () {
        $(this).addClass("Hover");
    }, function () {
        $(this).removeClass("Hover");
    }).click(function () {
        $(".PageHiddenMenuContainer").slideToggle();

        if ($(this).hasClass("Selected")) {
            $(this).removeClass("Selected");
        } else {
            $(this).addClass("Selected");
        }
    });
    // To Enable A Notification
    //$("#NoticeText").text('Notice: This is a notice that will by system wide. It will appear to users to notify them of updates that have been performed or are about to be updated.');
    //$(".NoticeContainer").show();
});
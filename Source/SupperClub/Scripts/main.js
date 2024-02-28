$("#subscribeToggle").submit(function (event) {
    //when we add other form fields we can do some more client-side validation here; not worth it for just the one required field?
    if (!isEmail() | !isPostCode)
        event.preventDefault();
    else
        Subscribe();

});

function isEmail() {
    var emailReg = new RegExp(/^(("[\w-\s]+")|([\w-]+(?:\.[\w-]+)*)|("[\w-\s]+")([\w-]+(?:\.[\w-]+)*))(@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$)|(@\[?((25[0-5]\.|2[0-4][0-9]\.|1[0-9]{2}\.|[0-9]{1,2}\.))((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\.){2}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[0-9]{1,2})\]?$)/i);
    var valid = emailReg.test($("#email"));

    if (!valid) {
        return false;
    } else {
        return true;
    }
}
;

function isPostCode() {
    // a regex for UK post codes could be used here for client side validation, if you can write / find one that you know will work.
    return true;
}
;


function Subscribe() {
    var email = $('#email').val();
    var postcode = $('#postcode');
    // not strictly neccessary to have this condition with the regex as well, but won't hurt
    if (email != '' && email != null) {
        $.ajax({
            type: "POST",
            url: '@Url.Action("Subscribe","Home")',
            data: {
                emailAddress: email,
                postcode: postcode
            },
            cache: false,
            dataType: 'json text',
            traditional: true,
            success: function (result) {
                if (result == "true") {
                    $('#subscribeToggle').hide();
                    $('h1').text("Welcome to the club!");
                    $('p').text("We'll let you know about any new events and any other news...redirecting you now!");
                    window.setTimeout(function () {
                        window.location.href = 'grubclub.com/pop-up-restaurants';
                    }, 2000);
                }
                else {
                    $('h1').text("Looks like there is a problem with your input");
                    $('p').text("please try again!");

                }
            },
            error: function () {
                alert("Could not save user newsletter sign-up input, try again later.");
            }
        });
    }
}

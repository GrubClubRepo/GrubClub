(function () {
    'use strict';
    debugger;
    // Array for IE8
    if (!Array.prototype.indexOf) {
        Array.prototype.indexOf = function (needle) {
            for (var i = 0; i < this.length; i++) {
                if (this[i] === needle) {
                    return i;
                }
            }
            return -1;
        };
    }
   
    var IFRAME_CONTAINER = document.getElementById(TP_INIT.Setting.iFrameContainerId === null ? 'gc_div' : TP_INIT.Setting.iFrameContainerId),
        IFRAME_HEIGHT = isNaN(parseInt(TP_INIT.Setting.height, 10)) ? 650 : parseInt(TP_INIT.Setting.height, 10),
        IFRAME_WIDTH = isNaN(parseInt(TP_INIT.Setting.width, 10)) ? 500 : parseInt(TP_INIT.Setting.width, 10),
        TP_CLIENT_ID = parseInt(TP_INIT.Setting.tpId),
        IFRAME_BASE_URL = "https://grubclub.com/tpi/",
        IFRAME_ACTION = "showEvents",
        IFRAME_PARAMS = "?sid=",
        IFRAME_SOURCE_URL = IFRAME_BASE_URL + IFRAME_ACTION + IFRAME_PARAMS + TP_INIT.Setting.tpId
        debugger;
        addiFrame();

        function addiFrame(srcUrl) {
        debugger;
        var iFrame = document.createElement('iFrame');
        iFrame.height = IFRAME_HEIGHT;
        iFrame.width = IFRAME_WIDTH;
        iFrame.style.border = '0';
        iFrame.style.background = 'transparent';
        iFrame.style.maxWidth = '100%';
        iFrame.frameBorder = '0';
        iFrame.allowTransparency = 'true';
        iFrame.name = 'gc_iframe';
        iFrame.src = (typeof srcUrl === 'undefined') ? IFRAME_SOURCE_URL : srcUrl;
        var myNode = document.getElementById(IFRAME_CONTAINER.id);
        while (myNode.firstChild) {
            myNode.removeChild(myNode.firstChild);
        }
        myNode.appendChild(iFrame);
}
        function rowEventSelect(eventId, availableSeats, multiSeating) {
            debugger;
            var seatId = 0;
            if (availableSeats <= 0)
                return false;
            else {
                if (multiSeating.toLowerCase() == 'true') {
                    if ($("input[type='radio'][name='seatingTime_" + eventId + "']").length > 0) {
                        seatId = $("input[type='radio'][name='seatingTime_" + eventId + "']:checked").val();
                        if (!seatId || seatId <= 0) {
                            alert("Please choose a seating before continuing!");
                            return false;
                        }
                    }
                }
                response.redirect(IFRAME_BASE_URL + "chooseTickets?eid=" + eventId + "&stId=" + seatId);
            }
        }
})();
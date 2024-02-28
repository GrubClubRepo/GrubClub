"use strict";
var $ = window.jQuery;
$(document).ready(function () {
    function map() {
        function e() {
            var e = new google.maps.LatLngBounds;
            $.each(d, function (n, t) { e.extend(t.position) }), m.fitBounds(e)
        }
        for (var n, t = [{}], o = [["<h6>London location1</h6>", 51.5286416, -.1015987], ["<h6>London location2</h6>", 51.5103761, -.0178279], ["<h6>London location3</h6>", 51.5466864, -.2354945], ["<h6>London location4</h6>", 51.5897938, -.1702632], ["<h6>London location5</h6>", 51.5949128, -.1077785]], i = "http://maps.google.com/mapfiles/ms/icons/", s = [i + "red-dot.png"], a = s.length, l = { anchor: new google.maps.Point(15, 33), url: i + "msmarker.shadow.png" }, r = new google.maps.StyledMapType(t, { name: "Styled Map" }), c = { zoom: 15, center: new google.maps.LatLng(32.870486, 13.195499), scrollwheel: !0, streetViewControl: !1, panControl: !1, disableDefaultUI: !1, mapTypeControlOptions: { mapTypeIds: [google.maps.MapTypeId.ROADMAP, "map"] } }, p = new google.maps.InfoWindow({ maxWidth: 160 }), m = new google.maps.Map(document.getElementById("map"), c), d = new Array, u = 0, g = 0; g < o.length; g++) n = new google.maps.Marker({ position: new google.maps.LatLng(o[g][1], o[g][2]), map: m, icon: s[u], shadow: l }), d.push(n), google.maps.event.addListener(n, "click", function (e, n) { return function () { p.setContent(o[n][0]), p.open(m, e) } }(n, g)), u++, u >= a && (u = 0); e(), m.mapTypes.set("map", r), m.setMapTypeId("map")
        if (initialize && results && results.length)
            initialize(results, centrePointLat, centrePointLng);
    }
    function map_single_pin() { for (var e, n = [{}], t = [["<h6>London location1</h6>", 51.5286416, -.1015987]], o = "https://maps.google.com/mapfiles/ms/icons/", i = [o + "red-dot.png"], s = i.length, a = { anchor: new google.maps.Point(15, 33), url: o + "msmarker.shadow.png" }, l = new google.maps.StyledMapType(n, { name: "Styled Map" }), r = { zoom: 15, center: new google.maps.LatLng(51.5286416, -.1015987), scrollwheel: !0, streetViewControl: !1, panControl: !1, disableDefaultUI: !1, mapTypeControlOptions: { mapTypeIds: [google.maps.MapTypeId.ROADMAP, "map-single-pin"] } }, c = new google.maps.InfoWindow({ maxWidth: 160 }), p = new google.maps.Map(document.getElementById("map-single-pin"), r), m = new Array, d = 0, u = 0; u < t.length; u++) e = new google.maps.Marker({ position: new google.maps.LatLng(t[u][1], t[u][2]), map: p, icon: i[d], shadow: a }), m.push(e), google.maps.event.addListener(e, "click", function (e, n) { return function () { c.setContent(t[n][0]), c.open(p, e) } }(e, u)), d++, d >= s && (d = 0); p.mapTypes.set("map-single-pin", l), p.setMapTypeId("map-single-pin") }
    function calculate_total_price() {
        var sum_value_array = [];
        $(".choose-menu-table .number-spinner input").each(function () {
            var e = parseInt($(this).val()),
                n = $(this).parent().parent("td").siblings().find(".menu-sum").attr("data-price"),
                t = $(this).parents("td").siblings().find(".menu-sum b");
            if (isNaN(e))
                t.html(0), sum_value_array.push(0);
            else { var o = e * n; t.html(o.toFixed(2)), sum_value_array.push(o) }
        });
        //, console.log(sum_value_array);
        var total = eval(sum_value_array.join("+")); $(".total-number b").html(total);
    }
    function getUrlVars() { { var e = {}; window.location.href.replace(/[?&]+([^=&]+)=([^&]*)/gi, function (n, t, o) { e[t] = o }) } return e }
    $(".js-open-menu").on("click", function () {
        console.log("click"), $(".menu-overlay").addClass("is-open")
    }),
    $(".js-close-menu").on("click", function () {
        $(".menu-overlay").removeClass("is-open")
    }), $(".tabs .tab-item").hide(), $(".tabs").each(function () { var e = $(this); e.find(".tab-item:first").show().addClass("active-tab"), e.find("ul li:first").addClass("active") }), $(".tabs > ul li a").click(function () { var e = $(this), n = e.parents(".tabs").find(".tab-item"); n.removeClass("active-tab"), e.parent().siblings().removeClass("active"), e.parent().addClass("active"); var t = e.attr("href"); var retval = (n.hide(), $(t).fadeIn().addClass("active-tab"), !1); scrollToElement(this); return false; }), $(".wishlist-button").on("click", function (e) { e.preventDefault(), $(this).toggleClass("is-active") }),
    tryontabinit(),
    $(".owl-carousel--single-image").owlCarousel({ loop: !0, nav: !0, items: 1, responsiveClass: !0, autoHeight: !0, navText: ["<svg role='img' class='icon'>\n                <use xlink:href='/content/images/icons/icons.svg#icon-arrow_17x31'></use>\n            </svg>", "<svg role='img' class='icon'>\n                <use xlink:href='/content/images/icons/icons.svg#icon-arrow_17x31'></use>\n            </svg>"] }), $(".owl-carousel--img-with-content").owlCarousel({ loop: !0, nav: !0, items: 1, dots: !0, dotsEach: !0, mouseDrag: !1, responsiveClass: !0, navText: ["<svg role='img' class='icon'>\n                <use xlink:href='/content/images/icons/icons.svg#icon-arrow_17x31'></use>\n            </svg>", "<svg role='img' class='icon'>\n                <use xlink:href='/content/images/icons/icons.svg#icon-arrow_17x31'></use>\n            </svg>"] }),
    $(".owl-carousel--multi-slide").owlCarousel({ loop: !0, margin: 20, responsiveClass: !0, responsive: { 0: { items: 1, nav: !0 }, 600: { items: 2, nav: !0 }, 1e3: { items: 3, nav: !0, loop: !0 } }, navText: ["<svg role='img' class='icon'>\n                <use xlink:href='/content/images/icons/icons.svg#icon-arrow_17x31'></use>\n            </svg>", "<svg role='img' class='icon'>\n                <use xlink:href='/content/images/icons/icons.svg#icon-arrow_17x31'></use>\n            </svg>"] });
    var config = { ".chosen-select": {}, ".chosen-select-deselect": { allow_single_deselect: !0 }, ".chosen-select-no-single": { disable_search_threshold: 10 }, ".chosen-select-no-results": { no_results_text: "Oops, nothing found!" }, ".chosen-select-width": { width: "95%" } }; for (var selector in config) $(selector).chosen(config[selector]); if ($(".open-popup").magnificPopup({
        type: "inline", midClick: !0, callbacks: {
        open: function () {
function e() { for (var e, n = [{}], t = [["<h6>London location1</h6>", 51.5286416, -.1015987]], o = "http://maps.google.com/mapfiles/ms/icons/", i = [o + "red-dot.png"], s = i.length, a = { anchor: new google.maps.Point(15, 33), url: o + "msmarker.shadow.png" }, l = new google.maps.StyledMapType(n, { name: "Styled Map" }), r = { zoom: 15, center: new google.maps.LatLng(51.5286416, -.1015987), scrollwheel: !0, streetViewControl: !1, panControl: !1, disableDefaultUI: !1, mapTypeControlOptions: { mapTypeIds: [google.maps.MapTypeId.ROADMAP, "map-single-pin"] } }, c = new google.maps.InfoWindow({ maxWidth: 160 }), p = new google.maps.Map(document.getElementById("map-single-pin"), r), m = new Array, d = 0, u = 0; u < t.length; u++) e = new google.maps.Marker({ position: new google.maps.LatLng(t[u][1], t[u][2]), map: p, icon: i[d], shadow: a }), m.push(e), google.maps.event.addListener(e, "click", function (e, n) { return function () { c.setContent(t[n][0]), c.open(p, e) } }(e, u)), d++, d >= s && (d = 0); p.mapTypes.set("map-single-pin", l), p.setMapTypeId("map-single-pin") }
        //$(".mfp-close").append("<span>\n              <svg role='img' class='icon'>\n                <use xlink:href='../content/images/icons/icons.svg#icon-X-close'></use>\n              </svg>\n              </span>"),
        $(".map--single-pin").length && e()
    }
    }
    }), $(".gallery").each(function () {
$(this).magnificPopup({
        delegate: "a", type: "image", gallery: { enabled: !0 }, removalDelay: 200, closeOnContentClick: !1, callbacks: {
        open: function () {
        $(".mfp-arrow-left, .mfp-arrow-right").append("<span class='mfp-prevent-close'></span>")
        //,$(".mfp-close").append("<span>\n                      <svg role='img' class='icon'>\n                        <use xlink:href='../content/images/icons/icons.svg#icon-X-close'></use>\n                      </svg>\n                      </span>")
    },
        buildControls: function () { this.contentContainer.append(this.arrowLeft.add(this.arrowRight)) }
    }
    })
    }),

        $(".tooltip-links .icon").on("click", function () {
        $(this).parent().toggleClass("is-active"),
        $(this).parent().hasClass("is-active") ? ($(".message-popup-wraper .messages-popup").addClass("is-hidden"), $(".messages-popup--added").clone().appendTo(".message-popup-wraper").addClass("is-active").delay(3100).queue(function () { $(this).remove() }), $(".messages-popup").on("click", function () { $(this).addClass("is-hidden") })) : ($(".message-popup-wraper .messages-popup").addClass("is-hidden"), $(".messages-popup--removed").clone().appendTo(".message-popup-wraper").addClass("is-active").delay(3100).queue(function () { $(this).remove() }), $(".messages-popup").on("click", function () { $(this).addClass("is-hidden") }))

    }), $(".word-count").on("focus change keydown keypress keyup blur", function () {
        
        var e = $(this), n = e.next().find(".words-left"), t = e.val(), o = /\s+/gi, i = t.trim().replace(o, " ").split(" ").length, s = 300; if (0 == t.length) return void n.html(0); if (i > s) { var a = e.val().split(/\s+/gi, s).join(" "); $(this).val(a + " ") } else n.html(i)
    }), $(".character-count").on("focus change keydown keypress keyup blur", function () { var e = $(this), n = e.next().find(".character-left"), t = e.val(), o = t.length;  return 0 == t.length ? void n.html(0) : void n.html(o) }),

        $(".newsletter__button").on("click", function () {
        $(this).parent().toggleClass("is-active")
    }),
        $(".close-btn").on("click", function () {
        $(this).parents(".newsletter").removeClass("is-active")
    }),
        $(".follow").on("click", function (e) {
        e.preventDefault(); var n = $(this), t = n.text(); "follow" == t ? n.html("unfollow").addClass("btn--unfollow") : n.html("follow").removeClass("btn--unfollow")
    }),
        $(".view-filters a").on("click", function (e) {
        e.preventDefault(), $(this).addClass("is-active").siblings().removeClass("is-active"); var n = $(this).attr("href");
        $(".search-result-wrapper").removeClass("is-active"), $(n).parent(".search-result-wrapper").addClass("is-active"), map()
    }),
        $(".range-slider").length && ($(".range-slider").noUiSlider({ start: [0, 100], step: 1, range: { min: 0, "50%": 25, max: 100 } }), $(".range-slider").Link("upper").to('-inline-<div class="range-slider__tooltip"></div>', function (e) { $(this).html("£<span>" + parseInt(e) + "</span>") }), $(".range-slider").Link("lower").to('-inline-<div class="range-slider__tooltip"></div>', function (e) { $(this).html("£<span>" + parseInt(e) + "</span>") })),
        $(".search-result--following .remove-button").on("click", function () {
        $(this).closest(".layout__item").remove()
    }),
        $(".map").length && map(), $(".map--single-pin").length && map_single_pin(),
        $(".js__datepicker").pickadate({ format: "d mmm yy", onClose: function () { setTimeout(this.close, 0) } }), $(".datepicker11").length && $(".datepicker12").length) { var from_$input = $(".datepicker11").pickadate(), from_picker = from_$input.pickadate("picker"), to_$input = $(".datepicker12").pickadate(), to_picker = to_$input.pickadate("picker"); from_picker.get("value") && to_picker.set("min", from_picker.get("select")), to_picker.get("value") && from_picker.set("max", to_picker.get("select")), from_picker.on("set", function (e) { e.select ? to_picker.set("min", from_picker.get("select")) : "clear" in e && to_picker.set("min", !1) }), to_picker.on("set", function (e) { e.select ? from_picker.set("max", to_picker.get("select")) : "clear" in e && from_picker.set("max", !1) }) } if ($(".datepicker21").length && $(".datepicker22").length) { var from_$input2 = $(".datepicker21").pickadate(), from_picker2 = from_$input2.pickadate("picker"), to_$input2 = $(".datepicker22").pickadate(), to_picker2 = to_$input2.pickadate("picker"); from_picker2.get("value") && to_picker2.set("min", from_picker2.get("select")), to_picker2.get("value") && from_picker2.set("max", to_picker2.get("select")), from_picker2.on("set", function (e) { e.select ? to_picker2.set("min", from_picker2.get("select")) : "clear" in e && to_picker2.set("min", !1) }), to_picker2.on("set", function (e) { e.select ? from_picker2.set("max", to_picker2.get("select")) : "clear" in e && from_picker2.set("max", !1) }) } $(".nano").nanoScroller(),
    $(".accordion").each(function () { $(this).find("article:first > div").show() }), $(".accordion article h3").on("click", function () { $(this).next().slideToggle().parent().toggleClass("accordion--active").siblings().removeClass("accordion--active").find("div").slideUp() }),
    
    $(".chooser-list .load-more").on("click", function (e) { 
        e.preventDefault();
        $(this).parent().hide().parent().addClass("is-active") });

    $(".to-top-button").on("click", function (e) { 
        e.preventDefault(), 
        $("html, body").animate({ scrollTop: 0 }, "slow") });

    $(".input--alfanumeric").bind("keypress", function (e) { 
        var n = new RegExp("^[-_ a-zA-Z0-9\b]+$"), 
        t = String.fromCharCode(e.charCode ? e.charCode : e.which); 
        return n.test(t) ? void 0 : (e.preventDefault(), !1) });

    $(".input--number").bind("keypress", function (e) { 
        var n = new RegExp("^[0-9\b]+$"), 
        t = String.fromCharCode(e.charCode ? e.charCode : e.which); 
        return n.test(t) ? void 0 : (e.preventDefault(), !1) });
    
    $(".price-spinner .price-spinner__up").on("click", function (e) { 
        e.preventDefault(); 
        var n = $(".price-spinner__price span"), 
        t = n.text(), 
        o = Number(t) + 10; 
        n.html(o), 
        calculate_total_price() });
    
    $(".price-spinner .price-spinner__down").on("click", function (e) {
        e.preventDefault();
        var n = $(".price-spinner__price span"),
        t = n.text(),
        o = Number(t) - 10;
        n.html(o), calculate_total_price()
    });

    var url = getUrlVars().url;
    if ("reviews" == url && $('.tabs a[href="#tab-14"]').trigger("click"), $(".complete-line").length)
    {
        var complete_length = $(".complete-line").attr("data-complete");
        $(".complete-line b").css("width", complete_length + "%")
    }
    if ($(".edit-download-guest-list td .icon-close").on("click", function () {
        $(this).parents("tr").fadeOut()
    }),
    $(".js-booking-countdown").length) { var timeLeft = (new Date).getTime() + 6e5; $(".js-booking-countdown").countdown(timeLeft, function (e) { $(this).html(e.strftime("%-Mm %-Ss")) }) }
}), $(window).load(function () {
    //$(".host-step__layout").delegate("h2", "mouseenter mouseleave", function ()
    //{
    //   // debugger;
    //    var e = $(this).offset().top; console.log(e);
    //    var n = $(this).next(".hidden-hint").html(), t = $(".host-step__layout").offset().top; $(".js-hint").css("top", e - t).find("p").text(n)
    //});
    $(".host-step__layout").delegate(".tip", "mouseenter mouseleave", function () {
        //debugger;
        var e = $(this).offset().top; console.log(e);
        var n = $(this).prev(".hidden-hint").html(), t = $(".host-step__layout").offset().top; $(".js-hint").css("top", e - t).find("p").text(n)
    });
 
    var e = $(".js-menu-ticket").clone().html(), n = "<div class='js-menu-ticket bgr-white p mb-'>" + e + "</div>"; $(".js-add-menu-ticket").on("click", function (e) { e.preventDefault(); var t = $(this).parent().prev(".js-menu-ticket"); $(n).insertAfter(t).hide().fadeIn(500) }), $(".js-add-ticket").on("click", function (e) { e.preventDefault(); var n = $(this).parent().prev(".js-ticket"); n.clone().insertAfter(n).hide().fadeIn(500).find("table").addClass("no-heading") }), $("body").delegate(".js-add-course", "click", function (e) { e.preventDefault(); var n = $(this).parent().prev(".js-course"); n.clone().insertAfter(n).hide().fadeIn(500) }), $(".js-add-time").on("click", function (e) { e.preventDefault(); var n = $(this).parents("tr").prev(); n.clone().insertAfter(n).hide().fadeIn(500).addClass("no-icon") }), $(window).on("resize", function () { var e = $(".header-main"), n = e.outerHeight(); $(".homepage").length && $(".hero").css("padding-top", n) }).trigger("resize"), $(window).on("scroll", function () { $(window).scrollTop() > 1 ? $("html").addClass("header-scrolled") : $("html").removeClass("header-scrolled") }).trigger("scroll")
});
function tryontabinit() {
    
    try {
        var hash = window.location.hash.replace('#', '');
        _initMin_elementName = '#tab' + hash;
        //$('#tab' + hash).click();
        $(_initMin_elementName).click();
       
        _initMin_intervalID = setInterval(invokeScrollOnElement, 2000);
    
    }
    catch (ex) {

    }
}
var _initMin_intervalID;
var _initMin_elementName;
function invokeScrollOnElement() {
    try {
        
        scrollToElement(_initMin_elementName);
        window.clearInterval(_initMin_intervalID);
    }
    catch (ex) { }
}
function scrollToElement(element) {
    
    if (element != "#tab") {
        var pos = parseInt($(element).offset().top) - 400;

        $('html, body').scrollTop(parseInt(pos));
        //$('html, body').animate({
        //    scrollTop: parseInt(pos)
        //}, 1000);
    }
}
function tryontabset(hash) {
    try {

        $('#tab' + hash).click();


    }
    catch (ex) {

    }
}
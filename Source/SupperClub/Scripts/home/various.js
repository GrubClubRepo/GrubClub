$(document).ready(function(){
		
	// dot dot dot
	$('.text-name, .text-left').dotdotdot({
	watch: "window"
	});
	
	// nolink
	$('.nolink').click(function () {return false;});
	
	// date-input
	$(".date-input").datepicker({ showOn: "both", dateFormat: "dd/mm/yy" });
	
// Menu
	// Resizing menu
	function menuMargin(){
		$('.head').removeClass('head-smaller1 head-mob');
		$('.right-block').css({'width': 140});
		$('.right-block .right-top-link a').css({'width': 23});
		
		$('.menu-item').css({'marginLeft': 0});
		$('.sub-menu').css({'width': 'auto', 'marginLeft':0});
		var n = $( ".menu-item" ).length;
		var winWidth = $(window).width();
		var allWidth = $('.head').width();
		var subWidth = $('.sub-menu').width();
		var spaceForMenu = allWidth - 362;
		
		var mWidth = $('.menu').width();
		var mSpace = (spaceForMenu - mWidth) / (n + 1);
		
		if (mSpace <= 29){
			$('.head').addClass('head-smaller1');
			var mWidth = $('.menu').width();
			var mSpace = (spaceForMenu - mWidth) / (n + 1);
			if (mSpace <= 29){
				$('.head').addClass('head-mob');
				var socNo = $( ".right-top-link" ).length;
				if (winWidth <= 446){
					var socWidth = winWidth - 225 + 18;
				}
				else if (winWidth <= 650){
					var socWidth = winWidth - 225 + 21;
				}
				else{
					var socWidth = winWidth - 225;
				};
				$('.right-block').css({'width': socWidth});
				if (winWidth <= 446){
					$('.right-block .right-top-link a').css({'width': socWidth / (socNo)}); // no mail icon
				}
				else{
					$('.right-block .right-top-link a').css({'width': socWidth / (socNo + 1)});
				};
			};
		};
		
		if ($('.head').hasClass('head-mob')) {
			$('.sub-menu').css({'right': 'auto', 'left': 'auto'});
		}
		else {
			$('.menu-item').css({'marginLeft': mSpace});
			$('.sub-menu').css({'right': - mSpace / 2, 'left': - mSpace / 2});
			setTimeout(function() { 
				//menuMargin();
			}, 500);
		}
		$('.right-block, .menu, .menu-button').animate({'opacity': 1}, 0);
	};
	menuMargin();
	$(window).resize(function(){
		menuMargin();
	});

	//Sub menu Hover
	$('.menu-item').mouseenter(subOn);
	$('.menu-item').mouseleave(subOff);
	function subOn(){
		$sub=$(this).children('.sub-menu');
		$sub.css({'display':'block'});
		$sub.css({'height': 'auto'});
		var height=$sub.height();
		$(this).addClass('r-on');
		$sub.children('.sub-item').stop().animate({'opacity': 1}, 250);
		if ($('.head').hasClass('head-mob')) {
			var winWidth = $(window).width();
			$sub.css({'height': 0});
			if (winWidth <= 446){
				$sub.stop().animate({'opacity': 1, 'height': height, 'marginTop':0}, 250);
			}
			else{
				$sub.stop().animate({'opacity': 1, 'height': height, 'marginTop':3}, 250);
			};
		}
		else{
			$sub.css({'margin-top': - height - 80});
			$sub.stop().animate({'margin-top': 44, 'opacity': 1}, 250);
		};
	};
	function subOff(){
		$sub=$(this).children('.sub-menu');
		$(this).removeClass('r-on');
		$sub.children('.sub-item').stop().animate({'opacity': 0}, 250);
		if ($('.head').hasClass('head-mob')) {	
			$sub.stop().animate({'opacity': 0, 'height': 0, 'marginTop':-33}, 250);
		}
		else{
			var height=$sub.height();
			$sub.stop().animate({'margin-top':- height - 80, 'opacity': 0}, 250);
		};
	};
	
	//Menu right hovers
	$('.menu-button, .right-top-link a').mouseenter(socialOn);
	$('.menu-button, .right-top-link a').mouseleave(socialOff);
	function socialOn(){
		var winWidth=$(window).width();
		if (winWidth >= 446){
			$(this).animate({'top':-4}, 250);
		}
	};
	function socialOff(){
		var winWidth=$(window).width();
		if (winWidth >= 446){
			$(this).animate({'top':0}, 250);
		}
	};
	
// Tiles
	//Tile Blur hover
	$('.photo-tile').mouseenter(tileOn);
	$('.photo-tile').mouseleave(tileOff);
	function tileOn(){
		var winWidth = $(window).width();
		if (winWidth <= 446){
			$(this).children('.canvas-box, .photo-darker').animate({'marginTop': 100}, 250);
			$(this).children().children('.photo-blur-canvas').animate({'marginTop': -100}, 250);
			$(this).children('.text-left').animate({'bottom': -1}, 250);
			$(this).children('.text-right').animate({'bottom': -5}, 250);
		}
		else{
			$(this).children('.canvas-box, .photo-darker').animate({'marginTop': 133}, 250);
			$(this).children().children('.photo-blur-canvas').animate({'marginTop': -133}, 250);
			$(this).children('.text-left').animate({'bottom': 5}, 250);
			$(this).children('.text-right').animate({'bottom': -5}, 250);
		};
	};
	function tileOff(){
		var winWidth = $(window).width();
		if (winWidth <= 446){
			$(this).children('.canvas-box, .photo-darker').animate({'marginTop': 90}, 250);
			$(this).children().children('.photo-blur-canvas').animate({'marginTop': -90}, 250);
			$(this).children('.text-left').animate({'bottom': 4}, 250);
			$(this).children('.text-right').animate({'bottom': 0}, 250);
		}
		else{
			$(this).children('.canvas-box, .photo-darker').animate({'marginTop': 123}, 250);
			$(this).children().children('.photo-blur-canvas').animate({'marginTop': -123}, 250);
			$(this).children('.text-left').animate({'bottom': 10}, 250);
			$(this).children('.text-right').animate({'bottom': 0}, 250);
		};
	};
	
	// Read more
	$('.read-more span').mouseenter(readOn);
	$('.read-more span').mouseleave(readOff);
	function readOn(){
		$(this).addClass('read-more-on');
	};
	function readOff(){
		$(this).removeClass('read-more-on');
	};
	
	//Tile link	
	$('.category-tile').click(clickA);
	function clickA() {
	  window.location = $(this).find("a").first().attr("href");
	  return false;
	};

	// Video click
	$('.video-button').click(videoShow);
	function videoShow() {
		$roadOne = $(this).parent().parent().parent().parent().parent();
		$roadTwo = $(this).parent().parent().parent().parent().children().children();
		$roadThree = $(this).parent();
		$roadFour = $(this).parent().parent().parent().parent().children();
		
		$roadOne.children('.text-top').animate({'opacity':0}, 250);
		$roadThree.children().animate({'opacity':0}, 250);
		$roadFour.children('.bx-pager').animate({'opacity':0}, 250);
		$roadTwo.children('.bx-next').animate({'marginRight':-50}, 250);
		$roadTwo.children('.bx-prev').animate({'marginLeft':-50}, 250);
		
		$roadThree.children('.video-x').css({'display':'inline'});
		$roadThree.children('.video-box').css({'display':'inline'});
		$roadThree.children('.video-box').animate({'opacity':1}, 250);
		$roadThree.children('.video-x').animate({'opacity':1}, 250);
		
		setTimeout(function() { 
			$roadOne.children('.text-top').css({'display':'none'});
			$roadFour.children('.bx-pager').css({'display':'none'});
		}, 250);
	};
	$('.video-button').mouseenter(videoOn);
	$('.video-button').mouseleave(videoOff);
	function videoOn(){
		$(this).addClass('video-button-on');
	};
	function videoOff(){
		$(this).removeClass('video-button-on');
	};
	
	$('.video-x').click(videoX);
	function videoX() {
		$roadOne = $(this).parent().parent().parent().parent().parent();
		$roadTwo = $(this).parent().parent().parent().parent().children().children();
		$roadThree = $(this).parent();
		$roadFour = $(this).parent().parent().parent().parent().children();
	
		$roadOne.children('.text-top').css({'display':'block'});
		$roadFour.children('.bx-pager').css({'display':'block'});
	
		$roadOne.children('.text-top').animate({'opacity':1}, 250);
		$roadThree.children().animate({'opacity':1}, 250);
		$roadFour.children('.bx-pager').animate({'opacity':1}, 250);
		$roadTwo.children('.bx-next').animate({'marginRight':0}, 250);
		$roadTwo.children('.bx-prev').animate({'marginLeft':0}, 250);
		
		$roadThree.children('.video-box').stop().animate({'opacity':0}, 250);
		$(this).stop().animate({'opacity':0}, 250);
	
		setTimeout(function() {
			$roadThree.children('.video-x').css({'display':'none'});
			$roadThree.children('.video-box').css({ 'display': 'none' });
            // commented out the following line in order to display the video after toggling the 'X' button.
			//$roadThree.children('.video-box').html('');
		}, 250);
	};
	
// Footer
	// jquery.waituntilexists.js - https://gist.github.com/buu700/4200601
	// For Twitter
	(function ($) {
	$.fn.waitUntilExists	= function (handler, shouldRunHandlerOnce, isChild) {
		var found	= 'found';
		var $this	= $(this.selector);
		var $elements	= $this.not(function () { return $(this).data(found); }).each(handler).data(found, true);
		
		if (!isChild)
		{
			(window.waitUntilExists_Intervals = window.waitUntilExists_Intervals || {})[this.selector] =
				window.setInterval(function () { $this.waitUntilExists(handler, shouldRunHandlerOnce, true); }, 500)
			;
		}
		else if (shouldRunHandlerOnce && $elements.length)
		{
			window.clearInterval(window.waitUntilExists_Intervals[this.selector]);
		}
		
		return $this;
	}
	 
	}(jQuery));
	
	//Twitter block
	$("iframe#twitter-widget-0").waitUntilExists(function(){
		function twHeight(){
			var winWidth=$(window).width();
			if (winWidth >= 446){
				var height=$('.footer-right').height();
				$('.twitter-timeline').css({'height': height - 63});
			}
			
			var width=$('.twitter-block').width();
			
			$("iframe#twitter-widget-0").contents().find('head').append('<style>img.u-photo.avatar{display:none !important;}</style>');
			
			if (width >= 311){
				$("iframe#twitter-widget-0").contents().find('head').append('<style>.tweet{margin:-15px 0 -32px -58px!Important; color:#666666;}</style>');
			}
			else{
				$("iframe#twitter-widget-0").contents().find('head').append('<style>.tweet{margin:-49px 0 -66px 0px!Important; color:#666666;}</style>');
				//$("iframe#twitter-widget-0").contents().find('head').append('<style>.p-nickname{position:relative !important; left:-35px; margin-bottom:-110px;}</style>');
				//$("iframe#twitter-widget-0").contents().find('head').append('<style>.permalink{display:none !important;}</style>');
			};
		};
		twHeight();
		setTimeout(function() { 
			twHeight();
		}, 2000);
		$(window).resize(function(){
			twHeight();
		});
		
		$("iframe#twitter-widget-0").contents().find('head').append('<style>.full-name, .verified, .load-more, .tweet-actions, .inline-media, .u-url.profile .p-nickname, .p-name,.dt-updated, .retweet-credit, .expand {display:none!important;}</style>');
		$("iframe#twitter-widget-0").contents().find('head').append('<style>.e-entry-title a, .p-nickname, .permalink {color:#00B6FD !important;}</style>');
		$("iframe#twitter-widget-0").contents().find('head').append('<style>.tweet {border:none!important; margin-top:-15px !Important;}</style>');
	});
	
	//Author
	$('.author').mouseenter(authorOn);
	$('.author').mouseleave(authorOff);
	function authorOn(){
		$(this).children('a').animate({'backgroundColor': '#FFA300'}, 1000);
		$(this).children('a').animate({'backgroundColor': '#5F8FA6'}, 1000);
	};
	function authorOff(){
		$(this).children('a').stop().animate({'backgroundColor': '#FF6B00'}, 250);
	};
	
	// button-bottom hover
	$('.button-bottom').mouseenter(buttonOn);
	$('.button-bottom').mouseleave(buttonOff);
	function buttonOn(){
		var winWidth=$(window).width();
		if (winWidth >= 446){
			$(this).animate({'paddingTop': '9', 'paddingBottom': '5'}, 200);
		}
	};
	function buttonOff(){
		var winWidth=$(window).width();
		if (winWidth >= 446){
			$(this).animate({'paddingTop': '6', 'paddingBottom': '2'}, 200);
		}
	};
	
	
});

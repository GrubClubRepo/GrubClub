$(document).ready(function(){

	$menuButton=$('.menu-button');
	$menuHide=$('.menu-click-hide');
	$menu=$('.menu');
	$author=$('.author');
	$all=$('.all-except-head');
	
	function anim1(arg1, arg2){
		arg1[0].animate({'left':arg2[0]},300);
	}
	function anim2(arg1, arg2){
		arg1[0].animate({'right':arg2[0]},300);
	}
	
	$menuButton.click(menuOn);
	$menuHide.click(menuOn);
	function menuOn(){
		var winWidth=$(window).width();
		var winHeight=$(document).height();
		if ($('.menu').hasClass('menu-all-on')){
			var mWidth=$menu.width();
			//$menuButton.removeClass('menu-button-on');
			anim2([$all],['0']);
			anim2([$menu],[ - mWidth]);
			anim1([$menuHide],['100%']);
			setTimeout(function() { 
				$menu.removeClass('menu-all-on');
			}, 300);
		}
		else{
			$menu.addClass('menu-all-on');
			var mWidth=$menu.width();
			$menu.css({'right': - mWidth });
			anim1([$menuHide],['0']);
			anim2([$menu],[ 0]);
			//$menuButton.addClass('menu-button-on');
			anim2([$all],[ mWidth ]);
		}
	};
});

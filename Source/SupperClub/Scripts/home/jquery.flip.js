/*
 * Flip! jQuery Plugin (http://lab.smashup.it/flip/)
 * @author Luca Manno (luca@smashup.it) [http://i.smashup.it]
 */
(function($) {

function int_prop(fx){
    fx.elem.style[ fx.prop ] = parseInt(fx.now,10) + fx.unit;
}

var throwError=function(message) {
    throw({name:"jquery.flip.js plugin error",message:message});
};

var isIE6orOlder=function() {
    return (/*@cc_on!@*/false && (typeof document.body.style.maxHeight === "undefined"));
};

var acceptHexColor=function(color) {
	if(color && color.indexOf("#")==-1 && color.indexOf("(")==-1){
		return "rgb("+colors[color].toString()+")";
	} else {
		return color;
	}
};

$.extend( $.fx.step, {
    borderTopWidth : int_prop,
    borderBottomWidth : int_prop,
    borderLeftWidth: int_prop,
    borderRightWidth: int_prop
});

$.fn.revertFlip = function(){
	return this.each( function(){
		var $this = $(this);
		$this.flip($this.data('flipRevertedSettings'));		
	});
};

$.fn.flip = function(settings){
    return this.each( function() {
        var $this=$(this), flipObj, $clone, dirOption, dirOptions, newContent, ie6=isIE6orOlder();

        if($this.data('flipLock')){
            return false;
        }
		
		var revertedSettings = {
			direction: (function(direction){
				switch(direction)
				{
				case "tb":
				  return "bt";
				case "bt":
				  return "tb";
				case "lr":
				  return "rl";
				case "rl":
				  return "lr";		  
				default:
				  return "bt";
				}
			})(settings.direction),
			bgColor: acceptHexColor(settings.color) || "red",
			color: acceptHexColor(settings.bgColor) || $this.css("background-color"),
			content: $this.html(),
			speed: settings.speed || 500,
            onBefore: settings.onBefore || function(){},
            onEnd: settings.onEnd || function(){},
            onAnimation: settings.onAnimation || function(){}
		};
		
		$this
			.data('flipRevertedSettings',revertedSettings)
			.data('flipLock',1)
			.data('flipSettings',revertedSettings);

        flipObj = {
            width: $this.width(),
            height: $this.height(),
            bgColor: acceptHexColor(settings.bgColor) || $this.css("background-color"),
            direction: settings.direction || "tb",
            toColor: acceptHexColor(settings.color) || "#000000", // any color here, after flip
            speed: settings.speed || 250, // SPEED 
            top: $this.offset().top,
            left: $this.offset().left,
            target: settings.content || null,
            transparent: "transparent",
            dontChangeColor: settings.dontChangeColor || false,
            onBefore: settings.onBefore || function(){},
            onEnd: settings.onEnd || function(){},
            onAnimation: settings.onAnimation || function(){}
        };

        // This is the first part of a trick to support
        // transparent borders using chroma filter for IE6
        // The color below is arbitrary, lets just hope it is not used in the animation
        ie6 && (flipObj.transparent="#123456");

        $clone= $this.css("visibility","hidden")
            .clone(true)
			.data('flipLock',1)
            .appendTo("body")
            .html("")
            .css({visibility:"visible",position:"absolute",left:flipObj.left,top:flipObj.top,margin:0,zIndex:9999,"-webkit-box-shadow":"0px 0px 0px #000","-moz-box-shadow":"0px 0px 0px #000"});

        var defaultStart=function() {
            return {
                backgroundColor: flipObj.transparent,
                fontSize:0,
                lineHeight:0,
                borderTopWidth:0,
                borderLeftWidth:0,
                borderRightWidth:0,
                borderBottomWidth:0,
                borderTopColor:flipObj.transparent,
                borderBottomColor:flipObj.transparent,
                borderLeftColor:flipObj.transparent,
                borderRightColor:flipObj.transparent,
				background: "none",
                borderStyle:'solid',
                height:0,
                width:0
            };
        };
        var defaultHorizontal=function() {
            var waist=(flipObj.height/100)*25;
            var start=defaultStart();
            start.width=flipObj.width;
            return {
                "start": start,
                "first": {
                    borderTopWidth: 0,
                    borderLeftWidth: waist,
                    borderRightWidth: waist,
                    borderBottomWidth: 0,
                    borderTopColor: '#999',
                    borderBottomColor: '#999',
                    top: (flipObj.top+(flipObj.height/2)),
                    left: (flipObj.left-waist)},
                "second": {
                    borderBottomWidth: 0,
                    borderTopWidth: 0,
                    borderLeftWidth: 0,
                    borderRightWidth: 0,
                    borderTopColor: flipObj.transparent,
                    borderBottomColor: flipObj.transparent,
                    top: flipObj.top,
                    left: flipObj.left}
            };
        };
        var defaultVertical=function() {
            var waist=(flipObj.height/100)*25;
            var start=defaultStart();
            start.height=flipObj.height;
            return {
                "start": start,
                "first": {
                    borderTopWidth: waist,
                    borderLeftWidth: 0,
                    borderRightWidth: 0,
                    borderBottomWidth: waist,
                    borderLeftColor: '#999',
                    borderRightColor: '#999',
                    top: flipObj.top-waist,
                    left: flipObj.left+(flipObj.width/2)},
                "second": {
                    borderTopWidth: 0,
                    borderLeftWidth: 0,
                    borderRightWidth: 0,
                    borderBottomWidth: 0,
                    borderLeftColor: flipObj.transparent,
                    borderRightColor: flipObj.transparent,
                    top: flipObj.top,
                    left: flipObj.left}
            };
        };

        dirOptions = {
            "tb": function () {
                var d=defaultHorizontal();
                d.start.borderTopWidth=flipObj.height;
                d.start.borderTopColor=flipObj.bgColor;
                d.second.borderBottomWidth= flipObj.height;
                d.second.borderBottomColor= flipObj.toColor;
                return d;
            },
            "bt": function () {
                var d=defaultHorizontal();
                d.start.borderBottomWidth=flipObj.height;
                d.start.borderBottomColor= flipObj.bgColor;
                d.second.borderTopWidth= flipObj.height;
                d.second.borderTopColor= flipObj.toColor;
                return d;
            },
            "lr": function () {
                var d=defaultVertical();
                d.start.borderLeftWidth=flipObj.width;
                d.start.borderLeftColor=flipObj.bgColor;
                d.second.borderRightWidth= flipObj.width;
                d.second.borderRightColor= flipObj.toColor;
                return d;
            },
            "rl": function () {
                var d=defaultVertical();
                d.start.borderRightWidth=flipObj.width;
                d.start.borderRightColor=flipObj.bgColor;
                d.second.borderLeftWidth= flipObj.width;
                d.second.borderLeftColor= flipObj.toColor;
                return d;
            }
        };

        dirOption=dirOptions[flipObj.direction]();

        // Second part of IE6 transparency trick.
        ie6 && (dirOption.start.filter="chroma(color="+flipObj.transparent+")");

        newContent = function(){
            var target = flipObj.target;
            return target && target.jquery ? target.html() : target;
        };

        $clone.queue(function(){
            flipObj.onBefore($clone,$this);
            $clone.html('').css(dirOption.start);
            $clone.dequeue();
        });

        $clone.animate(dirOption.first,flipObj.speed);

        $clone.queue(function(){
            flipObj.onAnimation($clone,$this);
            $clone.dequeue();
        });
        $clone.animate(dirOption.second,flipObj.speed);

        $clone.queue(function(){
            if (!flipObj.dontChangeColor) {
                $this.css({backgroundColor: flipObj.toColor});
            }
            $this.css({visibility: "visible"});

            var nC = newContent();
            if(nC){$this.html(nC);}
            $clone.remove();
            flipObj.onEnd($clone,$this);
            $this.removeData('flipLock');
            $clone.dequeue();
        });
    });
};
})(jQuery);




$(function(){
	$(".main-tile-front .read-more span").bind("click",function(){
		var $this = $(this);
		setTimeout(function() { 
			$("#flipbox").flip({
				direction:'rl', /* rl bt tb lr */
				color: $this.attr("rev"),
			});
			$(".main-tile-front").hide();
			$(".main-tile-back").show();
			setTimeout(function() { 
				$(".main-tile-back").animate({'opacity':1},500);
				$(".main-tile-back").children().children('.read-more-bg').animate({'bottom':-1, 'right':-1,},500);
			}, 250);
		}, 200);
		$(".main-tile-front").animate({'opacity':0},200);
		$(".main-tile-front").children().children('.read-more-bg').animate({'bottom':-21, 'right':-21,},200);
		return false;
	});
	$(".main-tile-back .read-more span").bind("click",function(){
		var $this = $(this);
		setTimeout(function() { 
			$("#flipbox").flip({
				direction:'lr', /* rl bt tb lr */
				color: $this.attr("rev"),
			})
			$(".main-tile-back").hide();
			$(".main-tile-front").show();
			setTimeout(function() { 
				$(".main-tile-front").animate({'opacity':1},500);
				$(".main-tile-front").children().children('.read-more-bg').animate({'bottom':-1, 'right':-1,},500);
			}, 250);
		}, 200);
		$(".main-tile-back").animate({'opacity':0},200);
		$(".main-tile-back").children().children('.read-more-bg').animate({'bottom':-21, 'right':-21,},200);
		return false;
	});	
});

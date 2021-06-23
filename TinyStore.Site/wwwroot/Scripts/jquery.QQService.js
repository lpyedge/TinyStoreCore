(function ($) {
    $.extend({
        qqservice: function (qqnumber, option) {
            option = $.extend({
                size: '1.3rem',
                bgcolor: '#1296db',
                color: '#ffffff',
                //location: 'right: 0.3rem;bottom: 1.8rem;',
                location: 'br',
                offset: { 'x': '0.3rem', 'y': '1.8rem'}
            }, option);
            var stylelocation = {};
            var spacewidth = ((window.innerWidth - document.body.clientWidth) / 2) + 'px';
            var halfwidth = (window.innerWidth / 2) + 'px';
            var halfheight = (window.innerHeight / 2) + 'px';
            switch (option.location) {
                case 'tl':
                    stylelocation = { "top": option.offset.y, "left": "calc(" + option.offset.x + " + " + spacewidth + ")" };
                    break;
                case 'tr':
                    stylelocation = { "top": option.offset.y, "right": "calc(" + option.offset.x + " + " + spacewidth + ")" };
                    break;
                case 'bl':
                    stylelocation = { "bottom": option.offset.y, "left": "calc(" + option.offset.x + " + " + spacewidth + ")" };
                    break;
                case 'br':
                    stylelocation = { "bottom": option.offset.y, "right": "calc(" + option.offset.x + " + " + spacewidth + ")" };
                    break;
                case 'b':
                    stylelocation = { "bottom": option.offset.y, "right": "calc(" + halfwidth + " - " + halfnumberwithunit(option.size) + ")" };
                    break;
                case 't':
                    stylelocation = {"top": option.offset.y ,"left": "calc(" + halfwidth + " - " + halfnumberwithunit(option.size) + ")" };
                    break;
                case 'l':
                    stylelocation = { "top": "calc(" + halfheight + " - " + halfnumberwithunit(option.size) + ")", "left": "calc(" + option.offset.x + " + " + spacewidth + ")" };
                    break;
                case 'r':
                    stylelocation = { "top": "calc(" + halfheight + " - " + halfnumberwithunit(option.size) + ")", "right": "calc(" + option.offset.x + " + " + spacewidth + ")" };
                    break;
                default:
            }
            var icon = '<svg t="1598086172294" class="icon" viewBox="0 0 1025 1024" version="1.1" xmlns="http://www.w3.org/2000/svg" p-id="43113" width="100%" height="100%"><path d="M512.009337 0C229.23422 0 0 229.23422 0 511.990663c0 282.775117 229.23422 512.009337 512.009337 512.009337 282.775117 0 512.009337-229.23422 512.009337-512.009337C1024 229.23422 794.76578 0 512.009337 0zM801.26464 668.859701c-21.737567 18.637536-49.955319-61.346999-54.007769-49.040249-9.879014 29.935842-14.529061 49.936644-43.643208 82.505644-1.550016 1.736764 33.670819 14.473036 43.643208 41.62632 9.542867 26.03279 28.143053 67.285613-93.486477 80.227308-71.375413 7.582004-122.936772-38.022067-128.09104-37.592545-9.524192 0.84037-5.284993 0-15.51883 0-8.366349 0-8.926595 0.616271-16.807397 0-2.166287-0.168074-25.883391 37.592545-131.975416 37.592545-82.225521 0-103.514891-51.748108-86.987617-80.227308 16.545949-28.460526 44.128755-36.752175 40.244378-41.252822-19.141758-22.185764-32.363577-45.921544-40.244378-67.360312-1.942188-5.341017-3.585578-10.532635-4.874145-15.51883-2.987982-11.447705-25.883391 67.229588-50.459541 49.040249-24.576149-18.170664-22.391188-64.40968-6.480185-108.687834 16.060402-44.614302 56.491529-87.585213 56.939726-97.072055 1.624715-35.295534-3.473529-41.159448 0-50.422191 7.750078-20.766473 17.19957-12.792297 17.19957-23.567706 0-135.729068 100.863057-245.761494 225.293819-245.761494 124.412088 0 225.275145 110.032426 225.275145 245.761494 0 5.210293 13.520617 0 19.982128 23.567706 1.325917 4.874145 2.259661 23.66108 0.672296 50.422191-0.746995 12.848321 34.268415 28.497875 52.364379 97.072055C828.436598 628.764722 810.321959 661.109624 801.26464 668.859701z" p-id="43114" fill="' + option.bgcolor + '"></path></svg>';
            if (qqnumber) {
                var $floatqqservice = $("#float_qq_service");
                if (!$floatqqservice.length || $floatqqservice.length === 0) {
                    $floatqqservice = $("<div id='float_qq_service'></div>");
                    $floatqqservice.css($.extend({
                        "z-index": "999",
                        "display": "none",
                        "border-radius": "100%",
                        "position": "fixed",
                        "background-color": option.color,
                        "width": option.size,
                        "height": option.size,
                    }, stylelocation));
                    var $aLink = $("<a></a>");
                    if (/(iPhone|iPad|iPod|iOS|Android)/i.test(navigator.userAgent)) { //移动端
                        $aLink.attr("href", "mqqwpa://im/chat?chat_type=wpa&uin=" + qqnumber + "&version=1&src_type=web&web_src=");
                    } else {
                        $aLink.attr("href", "http://wpa.qq.com/msgrd?V=3&uin=" + qqnumber + "&Site=&Menu=yes");
                    }
                    $aLink.attr("target", "_blank");
                    $aLink.css({ "width": "100%", "height": "100%", "display": "flex" });
                    $aLink.html(icon);
                    $aLink.appendTo($floatqqservice);
                    $floatqqservice.appendTo('body'); 
                }
                $floatqqservice.show();
            }
        }
    });
    function halfnumberwithunit(num) {
        var tempnum = Number.parseFloat(num);
        var unit = num.replace(tempnum, '');
        return tempnum / 2 + unit;
    }
})(jQuery);

(function ($) {
    $.fn.BootstrapCheck = function (opt) {
        opt = $.extend({ "position": "left" }, opt);
        if ($(this).length > 0) {
            this.each(function () {
                inputcheck($(arguments[1]));
            });
        }

        function inputcheck($item) {
            var $p = $item.parent();
            if ($p.length > 0 &&  $p[0].tagName != "LABEL" && !$p.hasClass("custome-control")) {
                var title = $item.attr("title");
                if (title == undefined) {
                    title = "";
                }
                $item.addClass("custom-control-input");
                $item.wrap("<label class='custome-control'></label>");
                $item.parent().append("<span class='custom-control-indicator'></span><span class='custom-control-description' >" + title + "</span>");
            }
            else if ($p.length > 0 && $p[0].tagName == "LABEL" && $p.hasClass("custome-control")) {
                var title = $item.attr("title");
                if (title == undefined) {
                    title = "";
                }
                $item.parent().find("span.custom-control-description").text(title);
            }
        }
    }
})(jQuery);
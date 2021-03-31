(function($) {
    $.fn.FormSet = function(JsonObj, AttrName) {
        var $this = $(this);
        AttrName = AttrName && typeof (AttrName) == "string" ? AttrName : "name";
        if ($this.length == 1) {
            var isjson = typeof (JsonObj) == "object" &&
                Object.prototype.toString.call(JsonObj).toLowerCase() == "[object object]" &&
                !JsonObj.length;
            if (isjson) {
                for (var name in JsonObj) {
                    if (JsonObj.hasOwnProperty(name)) {
                        if (typeof(JsonObj[name]) == "object" &&
                            Object.prototype.toString.call(JsonObj[name]).toLowerCase() == "[object object]" &&
                            !JsonObj[name].length) {
                            for (var subname in JsonObj[name]) {
                                if (JsonObj[name].hasOwnProperty(subname)) {
                                    ElementSet($this, AttrName, name + "." + subname, JsonObj[name][subname]);
                                }
                            }
                        } else {
                            ElementSet($this, AttrName, name, JsonObj[name]);
                        }
                    }
                }
            }
        }
    };

    function ElementSet($this, AttrName, name, value) {
        var $taget;
        //easyui
        $taget = $this.find("input[combo" + AttrName + "='" + name + "'].easyui-datebox");
        if ($taget.length > 0) {
            $taget.datebox('setValue', value);
            return;
        }
        $taget = $this.find("input[combo" + AttrName + "='" + name + "'].easyui-combobox");
        if ($taget.length > 0) {
            $taget.combobox('setValue', value);
            return;
        }
        $taget = $this.find("select[combo" + AttrName + "='" + name + "'].easyui-combobox");
        if ($taget.length > 0) {
            $taget.combobox('setValue', value);
            return;
        }
        $taget = $this.find("input[combo" + AttrName + "='" + name + "'].easyui-combo");
        if ($taget.length > 0) {
            $taget.combo('setValue', value);
            return;
        }
        $taget = $this.find("select[combo" + AttrName + "='" + name + "'].easyui-combo");
        if ($taget.length > 0) {
            $taget.combo('setValue', value);
            return;
        }
        $taget = $this.find("input[numberbox" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.numberbox('setValue', value);
            return;
        }
        $taget = $this.find("input[textbox" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.textbox('setValue', value);
            return;
        }
        //easyui

        //iCheck
        $taget = $this.find("div[class^='icheckbox'] > input[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            if (value) {
                $taget.iCheck('check');
            } else {
                $taget.iCheck('uncheck');
            }
            return;
        }
        //iCheck

        $taget = $this.find("textarea[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.val(value);
            return;
        }
        $taget = $this.find("select[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.val(value);
            return;
        }
        $taget = $this.find("input:checkbox[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            if (typeof($taget.attr("value")) == "undefined")
                $taget.prop("checked", value);
            else
                $taget.val(value);
            return;
        }
        $taget = $this.find("input:radio[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.filter("[value='" + value + "']").prop("checked", true);
            return;
        }
        $taget = $this.find("input:password[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.val('');
            return;
        }
        $taget = $this.find("input[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.val(value);
            return;
        }
        $taget = $this.find("span[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.text(value);
            return;
        }
        $taget = $this.find("label[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.text(value);
            return;
        }
        $taget = $this.find("div[" + AttrName + "='" + name + "']");
        if ($taget.length > 0) {
            $taget.html(value);
            return;
        }
    }


    $.fn.FormGet = function(AttrName) {
        var JsonObj = {};
        var $this = $(this);
        AttrName = AttrName && typeof (AttrName) == "string" ? AttrName : "name";
        if ($this.length == 1) {
            var $taget = $this.find("textarea[" + AttrName + "!='']");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        ObjSet(JsonObj, $(item).attr(AttrName), $(item).val());
                    });
            }
            $taget = $this.find("select[" + AttrName + "!='']");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        ObjSet(JsonObj, $(item).attr(AttrName), $(item).val());
                    });
            }
            $taget = $this.find("input[" + AttrName + "!='']");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        ObjSet(JsonObj, $(item).attr(AttrName), $(item).val());
                    });
            }
            $taget = $this.find("input:checkbox[" + AttrName + "!='']");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        if (typeof($(item).attr("value")) == "undefined")
                            ObjSet(JsonObj, $(item).attr(AttrName), $(item).prop("checked"));
                        else
                            ObjSet(JsonObj, $(item).attr(AttrName), $(item).val());

                    });
            }
            $taget = $this.find("input:radio[" + AttrName + "!='']:checked");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        ObjSet(JsonObj, $(item).attr(AttrName), $(item).val());
                    });
            }
            $taget = $this.find("span[" + AttrName + "!='']");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        ObjSet(JsonObj, $(item).attr(AttrName), $(item).text());
                    });
            }
            $taget = $this.find("label[" + AttrName + "!='']");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        ObjSet(JsonObj, $(item).attr(AttrName), $(item).text());
                    });
            }
            $taget = $this.find("div[" + AttrName + "!='']");
            if ($taget.length > 0) {
                $.each($taget,
                    function(index, item) {
                        ObjSet(JsonObj, $(item).attr(AttrName), $(item).html());
                    });
            }
        }
        return JsonObj;
    };

    function ObjSet(JsonObj, AttrName, Value) {
        if (AttrName) {
            if (AttrName.indexOf(".") > 0) {
                var attrnames = AttrName.split(".");
                if (!JsonObj[attrnames[0]])
                    JsonObj[attrnames[0]] = {};
                JsonObj[attrnames[0]][attrnames[1]] = Value;

            } else {
                JsonObj[AttrName] = Value;
            }
        }
    }

})(jQuery);
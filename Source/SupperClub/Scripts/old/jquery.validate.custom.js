/* Additional jquery date validator for dd/MM/yy */
jQuery.validator.addMethod(
    'date',
    function (value, element, params) {
        if (this.optional(element)) {
            return true;
        };
        var result = false;
        try {
            $.datepicker.parseDate('dd/mm/yy', value);
            result = true;
        } catch (err) {
            result = false;
        }
        return result;
    },
    ''
);
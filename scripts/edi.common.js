var edi = {
    processor: "page.aspx",
    formatDate: function (dt) {
        var d = new Date(parseInt(dt.replace("/Date(", "").replace(")/", ""), 10))
        var day = d.getDate();
        var month = d.getMonth() + 1;
        var year = d.getFullYear();
        return month + '/' + day + '/' + year;
    },
    common: {
        // called when the page is loaded.
        init: function (processor) {
            edi.processor = processor;
        },
    },
    invoices: {
        // init the invoices page.
        init: function() {
            edi.common.init("EDIInvoices.aspx");
            $(".invoice_confirm").click(function () {
                var action = $(this).data("action");
                var count = $(".invoice_status input[type='checkbox']:checked").length;

                if (count == 0) return;

                var text = "Are you sure you want to " + action + " this invoice?";
                if (count > 1) text = "Are you sure you want to " + action + " " + count + " invoices?";
                return confirm(text);
            });

            $("#invoice_status").click(function () {
                $('.invoice_status input').prop('checked', $(this).is(':checked'));
            });
        }
    },
    pos: {
        // save a purchase order
        save: function () {
            $.ajax({
                url: edi.processor,
                dataType: 'json',
                data: {
                    action: "po-save",
                    id: current_id,
                    reviewStatus: $("input[@name=pof-status]:checked").val(),
                    applicationID: $("input[@name=pof-application]:checked").val(),
                    notes: $("#pof-notes").val(),
                    account: $("#pof-account").val(),
                    poRequestNumber: $("#pof-po-request-number").val()
                }
            }).done(function (obj) {
                if (obj === 0) return;
                if ("error" in obj) {
                    alert("An error occurred while saving the " +
                        "purchase order. Error: " + obj.error);
                    return;
                }
            });
        },
        // get the details on a purchase order
        get: function () {
        }
    }
};
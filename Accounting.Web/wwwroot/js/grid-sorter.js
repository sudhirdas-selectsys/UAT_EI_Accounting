var TableSorter = (function () {

    var configs = {};

    function parseDate(value) {
        if (!value) return new Date(0);

        var parts = value.split('/');
        if (parts.length === 3) {
            return new Date(parts[2], parts[1] - 1, parts[0]);
        }

        return new Date(value);
    }

    function getCellValue(row, config) {
        var value = $(row).find(config.selector).first().text().trim();

        switch (config.type) {
            case "date":
                return parseDate(value);

            case "number":
                return parseFloat(value.replace(/[^0-9.-]+/g, "")) || 0;

            default:
                return value.toLowerCase();
        }
    }

    function sortTable($table, column, order, columnConfig) {
        var config = columnConfig[column];
        if (!config) return;

        var $tbody = $table.find("tbody");
        var rows = $tbody.find("tr").toArray();

        rows.sort(function (a, b) {
            var A = getCellValue(a, config);
            var B = getCellValue(b, config);

            if (A === B) return 0;

            return order === "asc"
                ? (A > B ? 1 : -1)
                : (A < B ? 1 : -1);
        });

        // Reorder rows in tbody
        $tbody.append(rows);
    }

    function toggleIcons($table, column, currentOrder) {
        var nextOrder = currentOrder === "asc" ? "desc" : "asc";

        // Toggle the icons: show the opposite of the current order
        $table.find('[data-column="' + column + '"][data-order="' + currentOrder + '"]').hide();
        $table.find('[data-column="' + column + '"][data-order="' + nextOrder + '"]').show();

        return nextOrder; // return the new order for sorting
    }

    function init(configName, columnConfig) {
        configs[configName] = columnConfig;
    }

    // CLICK HANDLER
    $(document).on('click', '[data-name="sort-icon"]', function () {
        var $icon = $(this);
        var column = $icon.data('column');
        var currentOrder = $icon.data('order');

        var $table = $icon.closest("table");
        var configName = $table.data("sort-config");

        var columnConfig = configs[configName];
        if (!columnConfig) return;

        // Toggle icon state and determine the new sort order
        var nextOrder = toggleIcons($table, column, currentOrder);

        // Sort the table based on the next order (asc or desc)
        sortTable($table, column, nextOrder, columnConfig);
    });

    // INITIAL STATE: show only the appropriate icon based on initial load
    $(document).ready(function () {
        $('[data-name="sort-icon"][data-order="desc"]').hide();
    });

    
    function autoWire(sortConfigMap) {
        $("table[data-sort-config]").each(function () {

            var $table = $(this);
            if ($table.data("sort-wired")) return;// added
            var configName = $table.data("sort-config");

            var allowedHeaders = sortConfigMap[configName] || [];

            var columnConfig = {};
            var $headers = $table.find("thead th");
            var $rows = $table.find("tbody tr");

            $headers.each(function (colIndex) {
                var $th = $(this);
                $th.find('[data-name="sort-icon"]').remove();//added
                var headerText = $th.clone().children().remove().end().text().trim();
                if (!headerText) return;

                if (allowedHeaders.length && !allowedHeaders.some(h => h.toLowerCase() === headerText.toLowerCase())) {
                    return;
                }

                var key = headerText.replace(/\s+/g, '');

                var type = "text";
                if (headerText.toLowerCase().includes("date")) type = "date";
                else if (
                    headerText.toLowerCase().includes("amount") ||
                    headerText.toLowerCase().includes("due")
                ) type = "number";

                var selector = `td:eq(${colIndex})`;

                columnConfig[key] = {
                    selector: selector,
                    type: type
                };

                $rows.each(function () {
                    $(this).find("td").eq(colIndex).attr("data-sort-name", key);
                });

                // Inject the sort icons with updated title logic
                var asc = `<span data-name="sort-icon" data-column="${key}" data-order="asc" class="sort-btn" title="Ascending Order">▲</span>`;
                var desc = `<span data-name="sort-icon" data-column="${key}" data-order="desc" class="sort-btn" title="Descending Order">▼</span>`;

                $th.append(desc + asc); // Initially show descending first
                // Commented
                //$('[data-name="sort-icon"][data-order="asc"]').hide();
                //$('[data-name="sort-icon"][data-order="desc"]').show();
                //added
                $th.find('[data-order="asc"]').hide();
                $th.find('[data-order="desc"]').show();
            });

            init(configName, columnConfig); // reuse existing init
        });
    }

    return {
        init: init,
        autoWire: autoWire
    };

})();
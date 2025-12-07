
//#region bootstrap Pagination for ag-grid
function updatePaginationControls(gridApi) {
    const totalPages = gridApi.paginationGetTotalPages();
    const currentPage = gridApi.paginationGetCurrentPage();

    // Create <ul> dynamically
    const ul = document.createElement('ul');
    ul.className = 'pagination pagination-sm mb-0';

    // Helper to create page item
    function createBootstrapPageItem(pageIndex, currentPage) {
        const li = document.createElement('li');
        li.className = 'page-item' + (pageIndex === currentPage ? ' active' : '');

        const btn = document.createElement('button');
        btn.className = 'page-link';
        btn.textContent = pageIndex + 1;

        btn.addEventListener('click', () => gridApi.paginationGoToPage(pageIndex));

        li.appendChild(btn);
        return li;
    }

    // Always show first page
    ul.appendChild(createBootstrapPageItem(0, currentPage));

    // Ellipsis if current page > 3
    if (currentPage > 3) {
        const li = document.createElement('li');
        li.className = 'page-item disabled';
        li.innerHTML = '<span class="page-link">...</span>';
        ul.appendChild(li);
    }

    // Sliding window around current page
    const start = Math.max(1, currentPage - 2);
    const end = Math.min(totalPages - 2, currentPage + 2);

    for (let i = start; i <= end; i++) {
        ul.appendChild(createBootstrapPageItem(i, currentPage));
    }

    // Ellipsis if current page far from last
    if (currentPage < totalPages - 4) {
        const li = document.createElement('li');
        li.className = 'page-item disabled';
        li.innerHTML = '<span class="page-link">...</span>';
        ul.appendChild(li);
    }

    // Always show last page if more than 1
    if (totalPages > 1) {
        ul.appendChild(createBootstrapPageItem(totalPages - 1, currentPage));
    }

    // Return the <ul> element
    return ul;
}

function createBootstrapPageItem(pageIndex, currentPage) {
    const activeClass = pageIndex === currentPage ? ' active' : '';
    return `<li class="page-item${activeClass}">
                <button class="page-link" onclick="gridApi.paginationGoToPage(${pageIndex})">
                    ${pageIndex + 1}
                </button>
            </li>`;
}
//#endregion bootstrap Pagination for ag-grid

//#region active inactive filter for ag-grid
class cStatusFilter {
    constructor() {
        this.selected = null; // true | false | null
    }

    init(params) {
        this.params = params;

        // Get custom labels if provided, otherwise fallback
        const activeLabel = params.activeLabel || "Active";
        const inactiveLabel = params.inactiveLabel || "Inactive";

        this.gui = document.createElement('div');
        this.gui.innerHTML = `
      <div class="d-flex gap-2 p-2">
        <button id="btnActive" class="btn btn-sm btn-success ag-btn-toggle">
          <i class="fa fa-check"></i>&nbsp;${activeLabel}
        </button>
        <button id="btnInactive" class="btn btn-sm btn-danger ag-btn-toggle">
          <i class="fa fa-times"></i>&nbsp;${inactiveLabel}
        </button>
        <button id="btnClear" class="btn btn-sm btn-secondary">
          <i class="fa fa-ban"></i>&nbsp;Clear
        </button>
      </div>
    `;

        const btnActive = this.gui.querySelector('#btnActive');
        const btnInactive = this.gui.querySelector('#btnInactive');
        const btnClear = this.gui.querySelector('#btnClear');

        const refreshBtns = () => {
            btnActive.classList.toggle('active', this.selected === true);
            btnInactive.classList.toggle('active', this.selected === false);
        };

        btnActive.addEventListener('click', () => {
            this.selected = true;
            refreshBtns();
            params.filterChangedCallback();
        });
        btnInactive.addEventListener('click', () => {
            this.selected = false;
            refreshBtns();
            params.filterChangedCallback();
        });
        btnClear.addEventListener('click', () => {
            this.selected = null;
            refreshBtns();
            params.filterChangedCallback();
        });
    }

    getGui() { return this.gui; }

    isFilterActive() { return this.selected !== null; }

    // ✅ Use Grid API to get the cell value for this column
    doesFilterPass(params) {
        const colId = this.params.column.getColId();

        // Get value safely
        let value;
        if (this.params.colDef.valueGetter) {
            value = this.params.colDef.valueGetter(params);
        } else {
            value = params.data[colId];
        }

        const val = this._toBool(value);
        return val === this.selected;
    }

    getModel() {
        return this.selected === null ? null : { selected: this.selected };
    }

    setModel(model) {
        this.selected = model ? !!model.selected : null;
    }

    // Normalize Y/N, true/false, 1/0, strings like "Active"/"Inactive"
    _toBool(v) {
        if (v === true || v === 1) return true;
        if (v === false || v === 0) return false;
        if (v == null) return false;

        const s = String(v).trim().toLowerCase();
        if (['y', 'yes', 'true', '1', 'active', 'enabled'].includes(s)) return true;
        if (['n', 'no', 'false', '0', 'inactive', 'disabled'].includes(s)) return false;
        return null;
    }
}
//#endregion active inactive filter for ag-grid

//#region digit only editor for ag-grid
class cDigitOnlyCellEditor {
    init(params) {
        params.allowDecimal ??= false;
        params.decimalPlaces ??= 2;

        this.input = document.createElement('input');
        this.input.type = 'text';
        this.input.className = params.inputClass || 'form-control form-control-sm text-right';
        this.input.style.width = '100%';
        this.input.style.height = '100%';
        this.input.style.boxSizing = 'border-box';
        this.input.value = params.value ?? '';

        // block non-digit keys
        this.input.addEventListener('keydown', e => {
            const allowedKeys = ['Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Tab', 'Home', 'End'];
            if (allowedKeys.includes(e.key) || e.ctrlKey || e.metaKey) return;

            const value = this.input.value;
            const selectionStart = this.input.selectionStart;
            const selectionEnd = this.input.selectionEnd;

            if (params.allowDecimal && e.key === '.') {
                if (value.includes('.')) e.preventDefault();
                return;
            }

            if (/^[0-9]$/.test(e.key)) {
                if (params.allowDecimal && value.includes('.')) {
                    const dotIndex = value.indexOf('.');
                    if (selectionStart > dotIndex) {
                        const decimalsAfter = value.length - dotIndex - 1;
                        const isReplacing = selectionEnd > selectionStart;
                        if (decimalsAfter >= params.decimalPlaces && !isReplacing) e.preventDefault();
                    }
                }
                return;
            }

            e.preventDefault();
        });

        // block pasting non-digit values
        this.input.addEventListener('paste', e => {
            const paste = (e.clipboardData || window.clipboardData).getData('text');
            if (!/^\d+$/.test(paste)) e.preventDefault();
        });

        // Optional: handle Enter key to stop editing
        this.input.addEventListener('keydown', e => {
            if (e.key === 'Enter') {
                params.api.stopEditing();
            }
        });
    }

    getGui() {
        return this.input;
    }

    afterGuiAttached() {
        this.input.focus();
        this.input.select();
    }

    getValue() {
        return this.input.value ? Number(this.input.value) : null;
    }

    isPopup() {
        return false; // inline editor
    }
}
//#endregion digit only editor for ag-grid

//#region text editor for ag-grid
class cTextCellEditor {
    init(params) {
        this.params = params;
        this.input = document.createElement('input');
        this.input.type = 'text';
        this.input.className = params.inputClass || 'form-control form-control-sm';
        this.input.style.width = '100%';
        this.input.style.height = '100%';
        this.input.style.boxSizing = 'border-box';
        this.input.value = params.value ?? '';

        // Optional: limit max length if specified
        if (params.column.colDef.maxLength) {
            this.input.maxLength = params.column.colDef.maxLength;
        }

        // Optional: auto-trim spaces if desired
        if (params.column.colDef.trimInput) {
            this.input.addEventListener('blur', () => {
                this.input.value = this.input.value.trim();
            });
        }

        // Optional: handle Enter key to stop editing
        this.input.addEventListener('keydown', e => {
            if (e.key === 'Enter') {
                params.api.stopEditing();
            }
        });
    }

    getGui() {
        return this.input;
    }

    afterGuiAttached() {
        this.input.focus();
        this.input.select();
    }

    getValue() {
        return this.input.value;
    }

    isPopup() {
        return false; // inline editing
    }
}
//#endregion text editor for ag-grid

//#region date editor for ag-grid
class cDateCellEditor {
    init(params) {
        // Default options
        params.dateFormat ??= "d/M/Y"; // use your desired format
        params.allowInput ??= false;

        this.params = params;

        // Create input
        this.input = document.createElement('input');
        this.input.type = 'text';
        this.input.className = params.inputClass || 'form-control form-control-sm text-center';
        this.input.style.width = '100%';
        this.input.style.height = '100%';
        this.input.style.boxSizing = 'border-box';

        // --- normalize the incoming value ---
        let initialValue = null;

        if (params.value) {
            const raw = params.value;

            // If AG Grid passes Date object → use it directly
            let parsed =
                raw instanceof Date
                    ? raw
                    : !isNaN(Date.parse(raw))
                        ? new Date(raw)
                        : null;

            if (parsed) {
                initialValue = flatpickr.formatDate(parsed, params.dateFormat); // display formatted
            }
        }

        this.input.value = initialValue || "";

        // Initialize flatpickr
        this.picker = flatpickr(this.input, {
            dateFormat: params.dateFormat,
            minDate: params.minDate || null,
            maxDate: params.maxDate || null,
            allowInput: params.allowInput,
            defaultDate: initialValue || null,
            altInput: false,
            onClose: () => params.api.stopEditing(),
            onReady: () => this.input.select()
        });

        // Stop editing when Enter / Escape pressed
        this.input.addEventListener('keydown', e => {
            if (e.key === 'Enter') params.api.stopEditing();
            if (e.key === 'Escape') params.api.stopEditing(true);
        });
    }

    getGui() {
        return this.input;
    }

    afterGuiAttached() {
        this.input.focus();
        this.picker.open();

        // THIS IS THE REAL FIX — catch clicks on the calendar
        const calendar = this.picker.calendarContainer;
        if (calendar) {
            calendar.addEventListener('mousedown', (e) => {
                e.stopPropagation();        // ← AG Grid never sees the click
                e.preventDefault();         // ← extra safety
                /*console.log("Blocked mousedown on flatpickr calendar");*/
            });
        }
    }

    getValue() {
        const dateStr = this.input.value.trim();
        if (!dateStr) return null;

        try {
            const parsed = this.picker.parseDate(dateStr, this.params.dateFormat);
            return this.picker.formatDate(parsed, this.params.dateFormat);
        } catch {
            return dateStr;
        }
    }

    isPopup() {
        return true;
    }

    destroy() {
        if (this.picker) this.picker.destroy();
    }
}
//#endregion date editor for ag-grid

//#region add row header button for ag-grid
class cAddRowHeader {
    init(params) {
        this.params = params;
        this.defaultValues = params.column.colDef.headerComponentParams?.defaultValues || {};

        this.eGui = document.createElement('div');
        this.eGui.innerHTML = `<span title="Add New Row" style="cursor:pointer;">
            <i class="fa fa-circle-plus fs-3"></i></span>`;

        this.eGui.addEventListener('click', () => this.addNewRow());
    }

    addNewRow() {
        const api = this.params.api;
        const columnApi = this.params.columnApi || api?.columnModel;
        if (!api) return;

        // 🧱 Step 1: Build base row with default (type-based) values
        const newRow = this._buildDefaultRow(api, columnApi);

        // 🧩 Step 2: Override desired columns from header params
        Object.assign(newRow, this.defaultValues);

        // Calculate correct insert index
        const lastRowIndex = this._getLastDataRowIndex(api);

        // 🧾 Step 3: Add row to grid
        api.applyTransaction({ add: [newRow], addIndex: lastRowIndex + 1 });

        // 🔄 Refresh grid
        api.refreshCells({ force: true });

        // 🎯 Focus first editable cell
        setTimeout(() => {
            const firstRowNode = api.getDisplayedRowAtIndex(0);
            if (firstRowNode && columnApi?.getAllGridColumns) {
                const firstEditableCol = columnApi
                    .getAllGridColumns()
                    .find(col => col.isCellEditable(firstRowNode));

                if (firstEditableCol) {
                    api.startEditingCell({
                        rowIndex: 0,
                        colKey: firstEditableCol.getColId(),
                    });
                }
            }
        }, 50);
    }

    // Returns the index of the last non-pinned row (ignores pinnedTop/pinnedBottom)
    _getLastDataRowIndex(api) {
        let lastIndex = -1;
        api.forEachNode(node => {
            if (!node.rowPinned) {
                lastIndex = node.rowIndex;
            }
        });
        return lastIndex; // will be -1 if no rows yet → new row goes to index 0 (correct)
    }

    _buildDefaultRow(api, columnApi) {
        const row = {};
        let cols = [];

        if (api.getColumnDefs) cols = api.getColumnDefs();
        else if (columnApi?.getAllGridColumns) cols = columnApi.getAllGridColumns();

        cols.forEach(col => {
            const field = col.field || col.colDef?.field;
            if (!field) return;
        });

        return row;
    }

    refresh(params) {
        if (params?.column?.colDef?.headerComponentParams) {
            this.defaultValues = params.column.colDef.headerComponentParams.defaultValues || {};
        }
        return true; // tell ag-Grid it can reuse this header
    }

    getGui() {
        return this.eGui;
    }
}
//#endregion add row header button for ag-grid

//#region delete row renderer for ag-grid
class cDeleteRowRenderer {
    init(params) {
        this.params = params;

        if (params.node.rowPinned) return;
        // Create delete icon button
        this.eGui = document.createElement('span');
        this.eGui.innerHTML = `
            <i class="fa fa-circle-minus text-danger fs-5" title="Delete Row" style="cursor:pointer;"></i>
        `;

        // Handle click — delete this row
        this.eGui.addEventListener('click', async () => {
            const api = this.params.api;
            const node = this.params.node;

            if (!api || !node) return;

            const colDefs = api.getColumnDefs();

            // Check only editable fields
            const editableFields = colDefs
                .filter(c => c.editable)
                .map(c => c.field)
                .filter(f => f); // ignore undefined/null fields

            // Determine if all editable fields are blank or whitespace
            const allBlank = editableFields.every(f => {
                const val = node.data[f];
                return val == null || String(val).trim() === '';
            });

            // Show confirm popup ONLY if all editable fields are not blank
            if (!allBlank) {

                // get visible columns (ignore hidden or pinned if needed)
                const visibleCols = api.getColumnDefs().filter(c => c.field && !c.hide);
                var validCols = visibleCols
                                // ✅ filter out blank or whitespace-only data
                                .filter(c => {
                                    const val = node.data[c.field];
                                    return val != null && String(val).trim() !== '';
                                });

                // build an HTML table for display
                const detailHtml = `
                  <div style="text-align:left; font-size:14px;">
                    <table style="width:100%; border-collapse:collapse;">
                      ${validCols.map(c => {
                          const header = c.headerName || c.field;
                          const cellValue = node.data[c.field];
                          const displayValue = c.valueFormatter ? c.valueFormatter({ value: cellValue }) : cellValue;
                        return `
                            <tr>
                              <td style="padding:4px 8px; border-bottom:1px solid #ddd;" class="fw-bold">${header}:</td>
                              <td style="padding:4px 8px; border-bottom:1px solid #ddd; text-align:right;">${displayValue}</td>
                            </tr>
                          `;
                                }).join('')}
                    </table>
                  </div>
                `;

                const result = await Swal.fire({
                    title: 'Remove Row?',
                    html: `
                        <p>Are you sure you want to remove the following row?<br>
                        This action <b>cannot</b> be reverted.</p>
                        ${detailHtml}
                    `,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Yes, remove it',
                    cancelButtonText: 'Cancel',
                    reverseButtons: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#6c757d',
                });

                if (!result.isConfirmed) return;
            }

            api.applyTransaction({ remove: [node.data] });

            api.refreshCells({ force: true });
        });
    }

    getGui() {
        return this.eGui;
    }

    refresh() {
        return false; // no need to refresh
    }
}
//#endregion delete row renderer for ag-grid
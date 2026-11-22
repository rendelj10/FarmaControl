// Interacciones de la interfaz FarmaControl. No contiene lógica de negocio del backend.
(() => {
    "use strict";

    const $ = (selector, root = document) => root.querySelector(selector);
    const $$ = (selector, root = document) => Array.from(root.querySelectorAll(selector));
    const token = () => $("#fc-antiforgery-token input[name='__RequestVerificationToken']")?.value
        || document.querySelector("input[name='__RequestVerificationToken']")?.value
        || "";

    const escapeHtml = value => String(value ?? "").replace(/[&<>"']/g, character => ({
        "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    })[character]);

    // Fechas comerciales (sin hora): conservar el día exacto registrado,
    // incluso si el navegador o la computadora están en otra zona horaria.
    const dateValue = value => {
        if (!value) return "—";
        const match = String(value).match(/^(\d{4})-(\d{2})-(\d{2})/);
        if (match) return `${match[3]}/${match[2]}/${match[1]}`;
        return escapeHtml(value);
    };

    // El backend guarda aperturas, cierres, pagos y movimientos en UTC.
    // Los campos SQL Server datetime suelen llegar como ISO sin Z: se interpretan explícitamente como UTC.
    const dateTimeManagua = value => {
        if (!value) return "—";
        const raw = String(value).trim();
        const iso = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(raw) ? raw : raw + "Z";
        const date = new Date(iso);
        if (Number.isNaN(date.getTime())) return escapeHtml(raw);
        return new Intl.DateTimeFormat("es-NI", {
            timeZone: "America/Managua", day: "2-digit", month: "2-digit", year: "numeric",
            hour: "numeric", minute: "2-digit", hour12: true
        }).format(date);
    };

    // Campos de tipo date representan días comerciales en Managua, no marcas de tiempo UTC.
    const todayManagua = () => {
        const parts = Object.fromEntries(new Intl.DateTimeFormat("en-US", {
            timeZone: "America/Managua", year: "numeric", month: "2-digit", day: "2-digit"
        }).formatToParts(new Date()).map(p => [p.type, p.value]));
        return `${parts.year}-${parts.month}-${parts.day}`;
    };

    const moneyValue = value => {
        if (value === null || value === undefined || value === "") return "—";
        return `C$ ${Number(value).toLocaleString("es-NI", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    };

    function feedback(root, message, type = "") {
        const target = $("[data-fc-feedback]", root);
        if (!target) return;
        target.textContent = message || "";
        target.className = `fc-feedback${type ? ` is-${type}` : ""}`;
    }

    async function api(url, options = {}) {
        const headers = new Headers(options.headers || {});
        headers.set("Accept", "application/json");
        headers.set("X-Requested-With", "XMLHttpRequest");

        const method = (options.method || "GET").toUpperCase();
        if (!["GET", "HEAD"].includes(method)) {
            const requestToken = token();
            if (requestToken) headers.set("RequestVerificationToken", requestToken);
        }

        const response = await fetch(url, {
            credentials: "same-origin",
            ...options,
            headers
        });

        const contentType = response.headers.get("content-type") || "";
        let payload = null;
        if (contentType.includes("application/json")) {
            payload = await response.json();
        } else {
            const text = await response.text();
            payload = text ? { message: text } : {};
        }

        if (response.status === 401) {
            window.location.assign("/Auth/Login");
            throw new Error("La sesión expiró. Inicia sesión nuevamente.");
        }
        if (response.status === 403) {
            throw new Error("No tienes permisos para realizar esta operación.");
        }
        if (!response.ok) {
            throw new Error(payload?.message || payload?.errorMessage || `La solicitud falló (${response.status}).`);
        }
        return payload;
    }

    function dataTableBody(search = "", start = 0, length = 10) {
        const body = new URLSearchParams();
        body.set("Draw", "1");
        body.set("Start", String(start));
        body.set("Length", String(length));
        body.set("Search.Value", search);
        body.set("Search.Regex", "false");
        return body;
    }

    async function getTableData(url, search = "", start = 0, length = 10) {
        return api(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
            body: dataTableBody(search, start, length)
        });
    }

    function responseRows(response) {
        return response?.data || response?.Data || response?.items || response?.Items || [];
    }

    function responseCount(response, rows) {
        return Number(response?.recordsFiltered ?? response?.RecordsFiltered ?? response?.recordsTotal ?? response?.RecordsTotal ?? rows.length);
    }

    function statusPill(value) {
        const normalized = String(value ?? "").toLowerCase();
        let css = "";
        if (["activo", "abierto", "recibida", "confirmada", "confirmado", "registrado"].includes(normalized)) css = "is-active";
        else if (["borrador", "pendiente"].includes(normalized)) css = "is-warning";
        else if (["anulado", "anulada", "inactivo", "cerrado"].includes(normalized)) css = "is-inactive";
        return `<span class="fc-status-pill ${css}">${escapeHtml(value)}</span>`;
    }

    function displayCell(row, column) {
        const value = row?.[column.key];
        if (column.format === "active") return statusPill(value ? "Activo" : "Inactivo");
        if (column.format === "status") return statusPill(value);
        if (column.format === "boolean") return value ? "Sí" : "No";
        if (column.format === "currency") return escapeHtml(moneyValue(value));
        if (column.format === "date") return dateValue(value);
        if (column.format === "datetimeUtc") return dateTimeManagua(value);
        if (column.format === "number") return value === null || value === undefined ? "—" : escapeHtml(Number(value).toLocaleString("es-NI"));
        if (value === null || value === undefined || value === "") return "—";
        return escapeHtml(value);
    }

    async function loadFieldOptions(field) {
        if (field.options) return field.options;
        if (field.source?.url) {
            const result = await api(field.source.url);
            return (Array.isArray(result) ? result : responseRows(result)).filter(item => item?.activo !== false);
        }
        if (field.source?.tableUrl) {
            const result = await getTableData(field.source.tableUrl, "", 0, 250);
            return responseRows(result).filter(item => item?.activo !== false);
        }
        return [];
    }

    function optionLabel(item, source) {
        if (source?.labelKeys?.length) {
            return source.labelKeys.map(key => item?.[key]).filter(Boolean).join(" · ");
        }
        return item?.[source?.labelKey || "nombre"] ?? item?.name ?? item?.Nombre ?? item?.id ?? "";
    }

    async function renderField(field, moduleKey, currentValue) {
        const id = `fc-${moduleKey}-${field.name}`;
        const value = currentValue ?? field.defaultValue ?? "";
        const required = field.required ? "required" : "";
        const maxLength = field.maxLength ? `maxlength="${field.maxLength}"` : "";
        const placeholder = escapeHtml(field.placeholder || "");
        const full = field.full ? "fc-field-full" : "";

        if (field.type === "select") {
            let options = [];
            try { options = await loadFieldOptions(field); } catch { options = []; }
            const selected = String(value ?? "");
            const blankText = field.placeholder || "Selecciona una opción";
            const optionsHtml = options.map(item => {
                const optionValue = item?.[field.source?.valueKey || "id"] ?? item?.id ?? item?.Id;
                const label = optionLabel(item, field.source);
                return `<option value="${escapeHtml(optionValue)}" ${String(optionValue) === selected ? "selected" : ""}>${escapeHtml(label)}</option>`;
            }).join("");
            return `<div class="fc-field ${full}"><label for="${id}">${escapeHtml(field.label)}</label><select id="${id}" name="${field.name}" class="fc-input" ${required}><option value="">${escapeHtml(blankText)}</option>${optionsHtml}</select></div>`;
        }

        if (field.type === "textarea") {
            return `<div class="fc-field ${full}"><label for="${id}">${escapeHtml(field.label)}</label><textarea id="${id}" name="${field.name}" class="fc-input" rows="${field.rows || 3}" placeholder="${placeholder}" ${required} ${maxLength}>${escapeHtml(value)}</textarea></div>`;
        }

        if (field.type === "checkbox") {
            return `<div class="fc-field fc-checkbox-field ${full}"><label for="${id}">${escapeHtml(field.label)}</label><label class="fc-checkbox-label"><input id="${id}" name="${field.name}" type="checkbox" ${value ? "checked" : ""} /> <span>${escapeHtml(field.checkText || "Sí")}</span></label></div>`;
        }

        const type = field.type || "text";
        const step = field.step ? `step="${field.step}"` : "";
        const min = field.min !== undefined ? `min="${field.min}"` : "";
        const max = field.max !== undefined ? `max="${field.max}"` : "";
        const inputValue = type === "date" ? String(value || "").slice(0, 10) : value;
        return `<div class="fc-field ${full}"><label for="${id}">${escapeHtml(field.label)}</label><input id="${id}" name="${field.name}" type="${type}" class="fc-input" value="${escapeHtml(inputValue)}" placeholder="${placeholder}" ${required} ${maxLength} ${step} ${min} ${max} /></div>`;
    }

    async function renderFields(container, fields, moduleKey, editing = false, record = {}) {
        if (!container) return;
        const activeFields = fields.filter(field => !(editing && field.createOnly) && !(!editing && field.editOnly));
        const html = [];
        for (const field of activeFields) html.push(await renderField(field, moduleKey, record?.[field.name]));
        container.innerHTML = html.join("");
    }

    function collectFields(form, fields) {
        const result = {};
        fields.forEach(field => {
            const input = form.elements?.namedItem(field.name) || form.querySelector?.(`[name="${field.name}"]`);
            if (!input) return;
            let value;
            if (field.type === "checkbox") value = input.checked;
            else if (field.type === "number") value = input.value === "" ? (field.required ? 0 : null) : Number(input.value);
            else if (field.type === "select" && /id$/i.test(field.name)) value = input.value === "" ? null : Number(input.value);
            else value = input.value.trim();

            if (typeof value === "string" && value === "" && !field.required) value = null;
            result[field.name] = value;
        });
        return result;
    }

    function validateContainer(container) {
        const controls = $$("input, select, textarea", container);
        for (const control of controls) {
            if (!control.checkValidity()) {
                control.reportValidity();
                return false;
            }
        }
        return true;
    }

    function resetControls(container) {
        $$("input, select, textarea", container).forEach(control => {
            if (control.type === "checkbox") control.checked = false;
            else control.value = "";
        });
    }

    const modules = {
        categorias: {
            controller: "Categoria", columns: [
                { key: "id", label: "ID" }, { key: "nombre", label: "Nombre" },
                { key: "descripcion", label: "Descripción" }, { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "nombre", label: "Nombre", type: "text", required: true, maxLength: 100, placeholder: "Ej. Analgésicos" },
                { name: "descripcion", label: "Descripción", type: "textarea", maxLength: 250, placeholder: "Descripción de la categoría" }
            ],
            idInBody: true
        },
        medicamentos: {
            controller: "Medicamento", idInBody: true, columns: [
                { key: "codigo", label: "Código" }, { key: "nombre", label: "Nombre" },
                { key: "principioActivo", label: "Principio activo" }, { key: "concentracion", label: "Concentración" },
                { key: "formaFarmaceutica", label: "Forma farmacéutica" }, { key: "categoriaNombre", label: "Categoría" },
                { key: "laboratorioNombre", label: "Laboratorio" }, { key: "presentacionNombre", label: "Presentación" },
                { key: "precioVenta", label: "Precio venta", format: "currency" }, { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "codigo", label: "Código", required: true, maxLength: 30, placeholder: "MED-001" },
                { name: "nombre", label: "Nombre", required: true, maxLength: 150, placeholder: "Nombre del medicamento" },
                { name: "principioActivo", label: "Principio activo", maxLength: 150 },
                { name: "concentracion", label: "Concentración", maxLength: 100 },
                { name: "formaFarmaceutica", label: "Forma farmacéutica", maxLength: 100 },
                { name: "categoriaId", label: "Categoría", type: "select", required: true, source: { url: "/Medicamento/GetCategorias", labelKey: "nombre" } },
                { name: "laboratorioId", label: "Laboratorio", type: "select", source: { url: "/Medicamento/GetLaboratorios", labelKey: "nombre" } },
                { name: "presentacionId", label: "Presentación", type: "select", source: { url: "/Medicamento/GetPresentaciones", labelKey: "nombre" } },
                { name: "precioVenta", label: "Precio de venta", type: "number", required: true, min: 0, step: "0.01", defaultValue: "0" }
            ]
        },
        lotes: {
            controller: "Lote", columns: [
                { key: "medicamentoNombre", label: "Medicamento" }, { key: "numeroLote", label: "N.º lote" },
                { key: "almacenNombre", label: "Almacén" }, { key: "cantidadInicial", label: "Cantidad inicial", format: "number" },
                { key: "existencias", label: "Existencias", format: "number" }, { key: "costoUnitario", label: "Costo unitario", format: "currency" },
                { key: "fechaIngreso", label: "Fecha ingreso", format: "date" }, { key: "fechaVencimiento", label: "Fecha vencimiento", format: "date" },
                { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "medicamentoId", label: "Medicamento", type: "select", required: true, source: { tableUrl: "/Medicamento/GetData", labelKeys: ["codigo", "nombre"] } },
                { name: "numeroLote", label: "Número de lote", required: true, maxLength: 50 },
                { name: "almacenId", label: "Almacén", type: "select", required: true, source: { url: "/Almacen/GetActivos", labelKeys: ["codigo", "nombre"] } },
                { name: "cantidadInicial", label: "Cantidad inicial", type: "number", required: true, min: 1, createOnly: true },
                { name: "costoUnitario", label: "Costo unitario", type: "number", required: true, min: .01, step: "0.01" },
                { name: "fechaIngreso", label: "Fecha de ingreso", type: "date", required: true },
                { name: "fechaVencimiento", label: "Fecha de vencimiento", type: "date", required: true }
            ],
            fieldsEdit: [
                { name: "medicamentoId", label: "Medicamento", type: "select", required: true, source: { tableUrl: "/Medicamento/GetData", labelKeys: ["codigo", "nombre"] } },
                { name: "numeroLote", label: "Número de lote", required: true, maxLength: 50 },
                { name: "almacenId", label: "Almacén", type: "select", required: true, source: { url: "/Almacen/GetActivos", labelKeys: ["codigo", "nombre"] } },
                { name: "costoUnitario", label: "Costo unitario", type: "number", required: true, min: .01, step: "0.01" },
                { name: "fechaIngreso", label: "Fecha de ingreso", type: "date", required: true },
                { name: "fechaVencimiento", label: "Fecha de vencimiento", type: "date", required: true }
            ]
        },
        proveedores: {
            controller: "Proveedor", columns: [
                { key: "codigo", label: "Código" }, { key: "razonSocial", label: "Razón social" },
                { key: "nombreComercial", label: "Nombre comercial" }, { key: "ruc", label: "RUC" },
                { key: "contacto", label: "Contacto" }, { key: "telefono", label: "Teléfono" },
                { key: "email", label: "Correo" }, { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "codigo", label: "Código", required: true, maxLength: 30 },
                { name: "razonSocial", label: "Razón social", required: true, maxLength: 150 },
                { name: "nombreComercial", label: "Nombre comercial", maxLength: 150 },
                { name: "ruc", label: "RUC", maxLength: 30 },
                { name: "contacto", label: "Persona de contacto", maxLength: 100 },
                { name: "telefono", label: "Teléfono", maxLength: 30 },
                { name: "email", label: "Correo electrónico", type: "email", maxLength: 150 },
                { name: "direccion", label: "Dirección", type: "textarea", full: true, maxLength: 250 }
            ]
        },
        clientes: {
            controller: "Cliente", columns: [
                { key: "codigo", label: "Código" }, { key: "nombre", label: "Nombre" },
                { key: "documentoIdentidad", label: "Documento" }, { key: "telefono", label: "Teléfono" },
                { key: "email", label: "Correo" }, { key: "direccion", label: "Dirección" },
                { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "codigo", label: "Código", required: true, maxLength: 30 },
                { name: "nombre", label: "Nombre completo", required: true, maxLength: 150 },
                { name: "documentoIdentidad", label: "Documento de identidad", maxLength: 30 },
                { name: "telefono", label: "Teléfono", maxLength: 30 },
                { name: "email", label: "Correo electrónico", type: "email", maxLength: 150 },
                { name: "direccion", label: "Dirección", type: "textarea", full: true, maxLength: 250 }
            ]
        },
        sucursales: {
            controller: "Sucursal", columns: [
                { key: "codigo", label: "Código" }, { key: "nombre", label: "Nombre" },
                { key: "direccion", label: "Dirección" }, { key: "telefono", label: "Teléfono" },
                { key: "email", label: "Correo" }, { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "codigo", label: "Código", required: true, maxLength: 30 },
                { name: "nombre", label: "Nombre", required: true, maxLength: 150 },
                { name: "direccion", label: "Dirección", type: "textarea", full: true, maxLength: 250 },
                { name: "telefono", label: "Teléfono", maxLength: 30 },
                { name: "email", label: "Correo electrónico", type: "email", maxLength: 150 }
            ]
        },
        almacenes: {
            controller: "Almacen", columns: [
                { key: "codigo", label: "Código" }, { key: "nombre", label: "Nombre" },
                { key: "sucursalNombre", label: "Sucursal" }, { key: "ubicacion", label: "Ubicación" },
                { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "sucursalId", label: "Sucursal", type: "select", required: true, source: { url: "/Sucursal/GetActivas", labelKeys: ["codigo", "nombre"] } },
                { name: "codigo", label: "Código", required: true, maxLength: 30 },
                { name: "nombre", label: "Nombre del almacén", required: true, maxLength: 120 },
                { name: "ubicacion", label: "Ubicación", type: "textarea", full: true, maxLength: 250 }
            ]
        },
        laboratorios: {
            controller: "Laboratorio", columns: [
                { key: "nombre", label: "Laboratorio" }, { key: "pais", label: "País" },
                { key: "contacto", label: "Contacto" }, { key: "telefono", label: "Teléfono" },
                { key: "email", label: "Correo" }, { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "nombre", label: "Nombre del laboratorio", required: true, maxLength: 150 },
                { name: "pais", label: "País", maxLength: 80 },
                { name: "contacto", label: "Contacto", maxLength: 100 },
                { name: "telefono", label: "Teléfono", maxLength: 30 },
                { name: "email", label: "Correo electrónico", type: "email", maxLength: 150 },
                { name: "direccion", label: "Dirección", type: "textarea", full: true, maxLength: 250 }
            ]
        },
        presentaciones: {
            controller: "Presentacion", columns: [
                { key: "nombre", label: "Presentación" }, { key: "descripcion", label: "Descripción" },
                { key: "cantidadContenido", label: "Contenido" }, { key: "unidadContenido", label: "Unidad" },
                { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "nombre", label: "Nombre", required: true, maxLength: 100 },
                { name: "descripcion", label: "Descripción", type: "textarea", full: true, maxLength: 250 },
                { name: "cantidadContenido", label: "Cantidad de contenido", type: "number", min: 0, step: "0.01" },
                { name: "unidadContenido", label: "Unidad de contenido", maxLength: 30 }
            ]
        },
        metodosPago: {
            controller: "MetodoPago", columns: [
                { key: "nombre", label: "Método de pago" }, { key: "descripcion", label: "Descripción" },
                { key: "requiereReferencia", label: "Requiere referencia", format: "boolean" },
                { key: "activo", label: "Activo", format: "active" }
            ],
            fields: [
                { name: "nombre", label: "Nombre del método", required: true, maxLength: 80 },
                { name: "descripcion", label: "Descripción", type: "textarea", full: true, maxLength: 250 },
                { name: "requiereReferencia", label: "Referencia obligatoria", type: "checkbox", checkText: "El método requiere una referencia" }
            ]
        },
        pagos: {
            controller: "Pago", createAction: "Create", noEdit: true,
            columns: [
                { key: "documento", label: "Documento" }, { key: "metodoPagoNombre", label: "Método de pago" },
                { key: "fechaPago", label: "Fecha y hora", format: "datetimeUtc" }, { key: "monto", label: "Monto", format: "currency" },
                { key: "referencia", label: "Referencia" }, { key: "estado", label: "Estado", format: "status" },
                { key: "usuarioNombre", label: "Registrado por" }
            ],
            fields: [
                { name: "compraId", label: "ID de compra (opcional)", type: "number", min: 1 },
                { name: "ventaId", label: "ID de venta (opcional)", type: "number", min: 1 },
                { name: "metodoPagoId", label: "Método de pago", type: "select", required: true, source: { url: "/MetodoPago/GetActivos", labelKey: "nombre" } },
                { name: "turnoCajaId", label: "ID de turno de caja (se completa si hay uno abierto)", type: "number", min: 1 },
                { name: "monto", label: "Monto", type: "number", required: true, min: .01, step: "0.01" },
                { name: "referencia", label: "Referencia", maxLength: 100 },
                { name: "observaciones", label: "Observaciones", type: "textarea", full: true, maxLength: 250 }
            ],
            customAction: { key: "anular", label: "Anular", endpoint: "Anular", statusKey: "estado", statusValue: "Registrado", confirm: "¿Deseas anular este pago?" }
        }
    };

    function setTableHead(root, columns, withActions = true) {
        const head = $("[data-fc-head]", root);
        if (!head) return;
        head.innerHTML = columns.map(column => `<th>${escapeHtml(column.label)}</th>`).join("")
            + (withActions ? "<th>Acciones</th>" : "");
    }

    function rowActionsHtml(row, config) {
        const id = row?.id ?? row?.Id;
        if (config.customAction) {
            const action = config.customAction;
            if (String(row?.[action.statusKey] ?? "").toLowerCase() !== String(action.statusValue).toLowerCase()) return "—";
            return `<button type="button" class="fc-action-link fc-action-danger" data-fc-action="${action.key}" data-id="${escapeHtml(id)}">${escapeHtml(action.label)}</button>`;
        }
        const parts = [];
        if (!config.noEdit) parts.push(`<button type="button" class="fc-action-link" data-fc-action="edit" data-id="${escapeHtml(id)}">Editar</button>`);
        if (config.deleteAction !== false && row?.activo !== false) parts.push(`<button type="button" class="fc-action-link fc-action-danger" data-fc-action="delete" data-id="${escapeHtml(id)}">Desactivar</button>`);
        return parts.length ? parts.join("") : "—";
    }

    async function initGenericModule(root) {
        const key = root.dataset.fcModule;
        const config = modules[key];
        if (!config) return;

        const controller = config.controller;
        const dataUrl = `/${controller}/GetData`;
        const body = $("[data-fc-body]", root);
        const form = $("[data-fc-form]", root);
        const fieldsContainer = $("[data-fc-fields]", root);
        const search = $("[data-fc-search]", root);
        const pageSize = 10;
        let page = 0;
        let total = 0;
        let currentRecord = null;

        const activeFields = editing => editing && config.fieldsEdit ? config.fieldsEdit : config.fields;

        async function completeTurnoAbierto() {
            if (key !== "pagos") return;
            const input = form?.elements.namedItem("turnoCajaId");
            if (!input || input.value) return;
            try {
                const turno = await api("/TurnoCaja/MiTurnoAbierto");
                if (turno?.id > 0 && !input.value) input.value = String(turno.id);
            } catch { /* No hay turno abierto o el usuario no tiene acceso; el campo permanece opcional. */ }
        }

        async function resetForm() {
            currentRecord = null;
            if (form) form.reset();
            const idInput = form?.elements.namedItem("__fcId");
            if (idInput) idInput.value = "";
            await renderFields(fieldsContainer, config.fields, key, false, {});
            await completeTurnoAbierto();
            const mode = $("[data-fc-mode]", root);
            if (mode) mode.textContent = "Nuevo registro";
            feedback(root, "");
        }

        async function loadRows() {
            if (!body) return;
            body.innerHTML = `<tr><td class="fc-table-loading" colspan="${config.columns.length + 1}">Cargando información...</td></tr>`;
            try {
                const response = await getTableData(dataUrl, search?.value || "", page * pageSize, pageSize);
                const rows = responseRows(response);
                total = responseCount(response, rows);
                setTableHead(root, config.columns, true);
                if (!rows.length) {
                    body.innerHTML = `<tr><td class="fc-empty-cell" colspan="${config.columns.length + 1}">No hay registros para mostrar.</td></tr>`;
                } else {
                    body.innerHTML = rows.map(row => `<tr>${config.columns.map(column => `<td>${displayCell(row, column)}</td>`).join("")}<td class="fc-actions-cell">${rowActionsHtml(row, config)}</td></tr>`).join("");
                }
                const count = $("[data-fc-count]", root);
                if (count) count.textContent = `${total} registro(s)`;
                const pageLabel = $("[data-fc-page]", root);
                if (pageLabel) pageLabel.textContent = `${page + 1} / ${Math.max(1, Math.ceil(total / pageSize))}`;
                const prev = $("[data-fc-prev]", root);
                const next = $("[data-fc-next]", root);
                if (prev) prev.disabled = page <= 0;
                if (next) next.disabled = (page + 1) * pageSize >= total;
            } catch (error) {
                body.innerHTML = `<tr><td class="fc-empty-cell" colspan="${config.columns.length + 1}">${escapeHtml(error.message)}</td></tr>`;
                feedback(root, error.message, "error");
            }
        }

        async function editRecord(id) {
            try {
                const record = await api(`/${controller}/GetById?id=${encodeURIComponent(id)}`);
                currentRecord = record;
                form?.reset();
                const fields = activeFields(true);
                await renderFields(fieldsContainer, fields, key, true, record);
                if (form?.elements.namedItem("__fcId")) form.elements.namedItem("__fcId").value = id;
                const mode = $("[data-fc-mode]", root);
                if (mode) mode.textContent = `Editando #${id}`;
                $("#fc-form-section", root)?.scrollIntoView({ behavior: "smooth", block: "start" });
                feedback(root, "");
            } catch (error) {
                feedback(root, error.message, "error");
            }
        }

        async function saveRecord(event) {
            event.preventDefault();
            if (!form.reportValidity()) return;
            const id = form.elements.namedItem("__fcId")?.value || "";
            const fields = activeFields(Boolean(id));
            const payload = collectFields(form, fields);
            if (id && config.idInBody) payload.id = Number(id);
            const endpoint = id ? (config.updateAction || "Update") : (config.createAction || "Create");
            const url = id && !config.idInBody ? `/${controller}/${endpoint}?id=${encodeURIComponent(id)}` : `/${controller}/${endpoint}`;

            try {
                const result = await api(url, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                });
                feedback(root, result?.message || "Operación guardada correctamente.", "success");
                await resetForm();
                page = 0;
                await loadRows();
            } catch (error) {
                feedback(root, error.message, "error");
            }
        }

        async function handleAction(event) {
            const button = event.target.closest("[data-fc-action]");
            if (!button) return;
            const id = button.dataset.id;
            const action = button.dataset.fcAction;
            if (action === "edit") return editRecord(id);

            if (action === "delete" || action === "anular") {
                const definition = action === "anular" ? config.customAction : null;
                const endpoint = definition?.endpoint || "Delete";
                const confirmText = definition?.confirm || "¿Deseas desactivar este registro?";
                if (!window.confirm(confirmText)) return;
                try {
                    const result = await api(`/${controller}/${endpoint}?id=${encodeURIComponent(id)}`, { method: "POST" });
                    feedback(root, result?.message || "Operación realizada correctamente.", "success");
                    await loadRows();
                } catch (error) {
                    feedback(root, error.message, "error");
                }
            }
        }

        await renderFields(fieldsContainer, config.fields, key, false, {});
        await completeTurnoAbierto();
        setTableHead(root, config.columns, true);
        await loadRows();
        $("[data-fc-new]", root)?.addEventListener("click", async () => {
            await resetForm();
            $("#fc-form-section", root)?.scrollIntoView({ behavior: "smooth", block: "start" });
        });
        $("[data-fc-cancel]", root)?.addEventListener("click", resetForm);
        form?.addEventListener("submit", saveRecord);
        body?.addEventListener("click", handleAction);
        search?.addEventListener("input", () => { page = 0; loadRows(); });
        $("[data-fc-prev]", root)?.addEventListener("click", () => { if (page > 0) { page--; loadRows(); } });
        $("[data-fc-next]", root)?.addEventListener("click", () => { if ((page + 1) * pageSize < total) { page++; loadRows(); } });
    }

    const transactions = {
        compras: {
            controller: "Compra",
            columns: [
                { key: "numeroDocumento", label: "N.º documento" }, { key: "proveedorNombre", label: "Proveedor" },
                { key: "almacenNombre", label: "Almacén" }, { key: "fechaCompra", label: "Fecha compra", format: "date" },
                { key: "fechaRecepcion", label: "Recepción (Nicaragua)", format: "datetimeUtc" }, { key: "total", label: "Total", format: "currency" },
                { key: "estado", label: "Estado", format: "status" }
            ],
            mainFields: [
                { name: "proveedorId", label: "Proveedor", type: "select", required: true, source: { url: "/Proveedor/GetActivos", labelKeys: ["codigo", "razonSocial"] } },
                { name: "almacenId", label: "Almacén destino", type: "select", required: true, source: { url: "/Almacen/GetActivos", labelKeys: ["codigo", "nombre"] } },
                { name: "numeroDocumento", label: "Número de documento", required: true, maxLength: 50 },
                { name: "fechaCompra", label: "Fecha de compra", type: "date", required: true, defaultValue: todayManagua() },
                { name: "observaciones", label: "Observaciones", type: "textarea", full: true, maxLength: 500 }
            ],
            detailFields: [
                { name: "medicamentoId", label: "Medicamento", type: "select", required: true, source: { tableUrl: "/Medicamento/GetData", labelKeys: ["codigo", "nombre"] } },
                { name: "cantidad", label: "Cantidad", type: "number", required: true, min: 1 },
                { name: "costoUnitario", label: "Costo unitario", type: "number", required: true, min: .01, step: "0.01" },
                { name: "numeroLote", label: "Número de lote", maxLength: 50 },
                { name: "fechaVencimiento", label: "Fecha de vencimiento", type: "date" }
            ],
            detailColumns: [
                { key: "medicamentoNombre", label: "Medicamento" }, { key: "cantidad", label: "Cantidad" },
                { key: "costoUnitario", label: "Costo unitario" }, { key: "numeroLote", label: "N.º lote" },
                { key: "fechaVencimiento", label: "Vencimiento" }
            ],
            action: "Recibir",
            actionLabel: "Recibir",
            actionStatus: "Borrador",
            createUrl: "/Compra/Create"
        },
        ventas: {
            controller: "Venta",
            columns: [
                { key: "numeroDocumento", label: "N.º documento" }, { key: "clienteNombre", label: "Cliente" },
                { key: "almacenNombre", label: "Almacén" }, { key: "fechaVenta", label: "Fecha venta", format: "date" },
                { key: "total", label: "Total", format: "currency" }, { key: "estado", label: "Estado", format: "status" }
            ],
            mainFields: [
                { name: "clienteId", label: "Cliente", type: "select", required: true, source: { url: "/Cliente/GetActivos", labelKey: "nombre" } },
                { name: "almacenId", label: "Almacén", type: "select", required: true, source: { url: "/Almacen/GetActivos", labelKeys: ["codigo", "nombre"] } },
                { name: "numeroDocumento", label: "Número de documento", required: true, maxLength: 50 },
                { name: "fechaVenta", label: "Fecha de venta", type: "date", required: true, defaultValue: todayManagua() },
                { name: "observaciones", label: "Observaciones", type: "textarea", full: true, maxLength: 500 }
            ],
            detailFields: [
                { name: "medicamentoId", label: "Medicamento", type: "select", required: true, source: { tableUrl: "/Medicamento/GetData", labelKeys: ["codigo", "nombre"] } },
                { name: "loteId", label: "Lote", type: "select", required: true, source: { tableUrl: "/Lote/GetData", labelKeys: ["numeroLote", "medicamentoNombre"] } },
                { name: "cantidad", label: "Cantidad", type: "number", required: true, min: 1 },
                { name: "precioUnitario", label: "Precio unitario", type: "number", required: true, min: .01, step: "0.01" }
            ],
            detailColumns: [
                { key: "medicamentoNombre", label: "Medicamento" }, { key: "numeroLote", label: "Lote" },
                { key: "cantidad", label: "Cantidad" }, { key: "precioUnitario", label: "Precio unitario" }
            ],
            action: "Confirmar",
            actionLabel: "Confirmar",
            actionStatus: "Borrador",
            createUrl: "/Venta/Create"
        }
    };

    async function initTransaction(root) {
        const key = root.dataset.fcTransaction;
        const config = transactions[key];
        if (!config) return;
        const controller = config.controller;
        const body = $("[data-fc-body]", root);
        const form = $("[data-fc-transaction-form]", root);
        const mainContainer = $("[data-fc-main-fields]", root);
        const detailContainer = $("[data-fc-detail-fields]", root);
        const search = $("[data-fc-search]", root);
        const pageSize = 10;
        let page = 0, total = 0, details = [];

        await renderFields(mainContainer, config.mainFields, key, false, {});
        await renderFields(detailContainer, config.detailFields, `${key}-detail`, false, {});
        setTableHead(root, config.columns, true);

        function renderDetails() {
            const detailBody = $("[data-fc-detail-body]", root);
            const head = $("[data-fc-detail-head]", root);
            if (head) head.innerHTML = config.detailColumns.map(col => `<th>${escapeHtml(col.label)}</th>`).join("") + "<th></th>";
            if (!detailBody) return;
            if (!details.length) {
                detailBody.innerHTML = `<tr><td colspan="${config.detailColumns.length + 1}" class="fc-empty-cell">Todavía no has agregado productos.</td></tr>`;
                return;
            }
            detailBody.innerHTML = details.map((item, index) => `<tr>${config.detailColumns.map(col => `<td>${escapeHtml(item[col.key] ?? "—")}</td>`).join("")}<td><button type="button" class="fc-action-link fc-action-danger" data-remove-detail="${index}">Quitar</button></td></tr>`).join("");
        }

        async function loadRows() {
            try {
                const result = await getTableData(`/${controller}/GetData`, search?.value || "", page * pageSize, pageSize);
                const rows = responseRows(result);
                total = responseCount(result, rows);
                body.innerHTML = rows.length ? rows.map(row => {
                    const status = String(row.estado || "").toLowerCase();
                    const canAct = status === "borrador";
                    const actionButtons = canAct
                        ? `<button type="button" class="fc-action-link" data-transaction-action="${config.action.toLowerCase()}" data-id="${escapeHtml(row.id)}">${config.actionLabel}</button><button type="button" class="fc-action-link fc-action-danger" data-transaction-action="anular" data-id="${escapeHtml(row.id)}">Anular</button>`
                        : "—";
                    return `<tr>${config.columns.map(col => `<td>${displayCell(row, col)}</td>`).join("")}<td>${actionButtons}</td></tr>`;
                }).join("") : `<tr><td colspan="${config.columns.length + 1}" class="fc-empty-cell">No hay operaciones registradas.</td></tr>`;
                $("[data-fc-count]", root).textContent = `${total} registro(s)`;
                $("[data-fc-page]", root).textContent = `${page + 1} / ${Math.max(1, Math.ceil(total / pageSize))}`;
                $("[data-fc-prev]", root).disabled = page <= 0;
                $("[data-fc-next]", root).disabled = (page + 1) * pageSize >= total;
            } catch (error) {
                body.innerHTML = `<tr><td colspan="${config.columns.length + 1}" class="fc-empty-cell">${escapeHtml(error.message)}</td></tr>`;
                feedback(root, error.message, "error");
            }
        }

        function collectDetail() {
            const item = collectFields(detailContainer, config.detailFields);
            const selectedMedicine = detailContainer.querySelector(`[name="medicamentoId"]`);
            const medicineLabel = selectedMedicine?.selectedOptions?.[0]?.textContent || "Medicamento";
            item.medicamentoNombre = medicineLabel;
            if (key === "ventas") {
                const selectedLot = detailContainer.querySelector(`[name="loteId"]`);
                item.numeroLote = selectedLot?.selectedOptions?.[0]?.textContent || "";
            }
            return item;
        }

        $("[data-fc-add-detail]", root)?.addEventListener("click", () => {
            if (!validateContainer(detailContainer)) return;
            const item = collectDetail();
            if (key === "compras") {
                if (!item.medicamentoId || !item.cantidad || !item.costoUnitario) return;
            } else if (!item.medicamentoId || !item.loteId || !item.cantidad || !item.precioUnitario) return;
            details.push(item);
            resetControls(detailContainer);
            renderDetails();
        });

        $("[data-fc-detail-body]", root)?.addEventListener("click", event => {
            const button = event.target.closest("[data-remove-detail]");
            if (!button) return;
            details.splice(Number(button.dataset.removeDetail), 1);
            renderDetails();
        });

        form?.addEventListener("submit", async event => {
            event.preventDefault();
            if (!validateContainer(mainContainer)) return;
            if (!details.length) {
                feedback(root, "Agrega al menos un producto al detalle.", "error");
                return;
            }
            const payload = collectFields(mainContainer, config.mainFields);
            payload.detalles = details.map(item => {
                if (key === "compras") {
                    return {
                        medicamentoId: item.medicamentoId, cantidad: item.cantidad,
                        costoUnitario: item.costoUnitario, numeroLote: item.numeroLote,
                        fechaVencimiento: item.fechaVencimiento
                    };
                }
                return {
                    medicamentoId: item.medicamentoId, loteId: item.loteId,
                    cantidad: item.cantidad, precioUnitario: item.precioUnitario
                };
            });
            try {
                const result = await api(config.createUrl, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                });
                feedback(root, result?.message || "Operación registrada correctamente.", "success");
                form.reset();
                details = [];
                renderDetails();
                await renderFields(mainContainer, config.mainFields, key, false, {});
                await renderFields(detailContainer, config.detailFields, `${key}-detail`, false, {});
                page = 0;
                await loadRows();
            } catch (error) {
                feedback(root, error.message, "error");
            }
        });

        body?.addEventListener("click", async event => {
            const button = event.target.closest("[data-transaction-action]");
            if (!button) return;
            const id = button.dataset.id;
            const action = button.dataset.transactionAction;
            const endpoint = action === "anular" ? "Anular" : config.action;
            const prompt = action === "anular"
                ? "¿Deseas anular este borrador?"
                : (key === "compras" ? "¿Confirmas la recepción de esta compra? Las existencias se actualizarán." : "¿Confirmas esta venta? Las existencias se descontarán.");
            if (!window.confirm(prompt)) return;
            try {
                const result = await api(`/${controller}/${endpoint}?id=${encodeURIComponent(id)}`, { method: "POST" });
                feedback(root, result?.message || "Operación actualizada.", "success");
                await loadRows();
            } catch (error) { feedback(root, error.message, "error"); }
        });

        $("[data-fc-new]", root)?.addEventListener("click", () => {
            form?.scrollIntoView({ behavior: "smooth", block: "start" });
        });
        $("[data-fc-clear]", root)?.addEventListener("click", async () => {
            form?.reset(); details = []; renderDetails();
            await renderFields(mainContainer, config.mainFields, key, false, {});
            await renderFields(detailContainer, config.detailFields, `${key}-detail`, false, {});
            feedback(root, "");
        });
        search?.addEventListener("input", () => { page = 0; loadRows(); });
        $("[data-fc-prev]", root)?.addEventListener("click", () => { if (page > 0) { page--; loadRows(); } });
        $("[data-fc-next]", root)?.addEventListener("click", () => { if ((page + 1) * pageSize < total) { page++; loadRows(); } });
        renderDetails();
        await loadRows();
    }

    async function initAdjustment(root) {
        const body = $("[data-fc-body]", root);
        const form = $("[data-fc-adjustment-form]", root);
        const main = $("[data-fc-adjustment-main]", root);
        const detail = $("[data-fc-adjustment-detail]", root);
        const search = $("[data-fc-search]", root);
        const fields = [
            { name: "almacenId", label: "Almacén", type: "select", required: true, source: { url: "/Almacen/GetActivos", labelKeys: ["codigo", "nombre"] } },
            { name: "tipoAjuste", label: "Tipo de ajuste", type: "select", required: true, options: [{ id: "Conteo", nombre: "Conteo" }, { id: "Correccion", nombre: "Corrección" }], source: { valueKey: "id", labelKey: "nombre" } },
            { name: "fechaAjuste", label: "Fecha del ajuste", type: "date", required: true, defaultValue: todayManagua() },
            { name: "motivo", label: "Motivo", required: true, maxLength: 250 },
            { name: "observaciones", label: "Observaciones", type: "textarea", full: true, maxLength: 500 }
        ];
        const detailFields = [
            { name: "loteId", label: "Lote", type: "select", required: true, source: { tableUrl: "/Lote/GetData", labelKeys: ["numeroLote", "medicamentoNombre"] } },
            { name: "cantidadFisica", label: "Cantidad física", type: "number", required: true, min: 0 },
            { name: "observacion", label: "Observación", maxLength: 250 }
        ];
        await renderFields(main, fields, "ajuste", false, {});
        await renderFields(detail, detailFields, "ajuste-detail", false, {});
        const details = [];
        let page = 0, total = 0;
        const pageSize = 10;
        const columns = [
            { key: "almacenNombre", label: "Almacén" }, { key: "tipoAjuste", label: "Tipo" },
            { key: "fechaAjuste", label: "Fecha", format: "date" }, { key: "motivo", label: "Motivo" },
            { key: "usuarioNombre", label: "Registrado por" }, { key: "estado", label: "Estado", format: "status" }
        ];
        setTableHead(root, columns, true);

        function renderDetailRows() {
            const target = $("[data-fc-adjustment-details]", root);
            if (!details.length) {
                target.innerHTML = `<tr><td colspan="4" class="fc-empty-cell">No hay lotes agregados.</td></tr>`;
                return;
            }
            target.innerHTML = details.map((item, index) => `<tr><td>${escapeHtml(item.loteNombre)}</td><td>${escapeHtml(item.cantidadFisica)}</td><td>${escapeHtml(item.observacion || "—")}</td><td><button type="button" class="fc-action-link fc-action-danger" data-adjust-remove="${index}">Quitar</button></td></tr>`).join("");
        }

        async function loadRows() {
            try {
                const response = await getTableData("/AjusteInventario/GetData", search?.value || "", page * pageSize, pageSize);
                const rows = responseRows(response);
                total = responseCount(response, rows);
                body.innerHTML = rows.length ? rows.map(row => {
                    const actions = String(row.estado).toLowerCase() === "borrador"
                        ? `<button type="button" class="fc-action-link" data-adjust-action="confirmar" data-id="${escapeHtml(row.id)}">Confirmar</button><button type="button" class="fc-action-link fc-action-danger" data-adjust-action="anular" data-id="${escapeHtml(row.id)}">Anular</button>`
                        : "—";
                    return `<tr>${columns.map(col => `<td>${displayCell(row, col)}</td>`).join("")}<td>${actions}</td></tr>`;
                }).join("") : `<tr><td colspan="${columns.length + 1}" class="fc-empty-cell">No hay ajustes registrados.</td></tr>`;
                $("[data-fc-count]", root).textContent = `${total} ajuste(s)`;
                $("[data-fc-page]", root).textContent = `${page + 1} / ${Math.max(1, Math.ceil(total / pageSize))}`;
                $("[data-fc-prev]", root).disabled = page <= 0;
                $("[data-fc-next]", root).disabled = (page + 1) * pageSize >= total;
            } catch (error) {
                body.innerHTML = `<tr><td colspan="${columns.length + 1}" class="fc-empty-cell">${escapeHtml(error.message)}</td></tr>`;
                feedback(root, error.message, "error");
            }
        }

        $("[data-fc-adjustment-add]", root)?.addEventListener("click", () => {
            if (!validateContainer(detail)) return;
            const item = collectFields(detail, detailFields);
            item.loteNombre = detail.elements.loteId.selectedOptions[0]?.textContent || "";
            if (!item.loteId || item.cantidadFisica === null) return;
            details.push(item);
            resetControls(detail);
            renderDetailRows();
        });
        $("[data-fc-adjustment-details]", root)?.addEventListener("click", event => {
            const button = event.target.closest("[data-adjust-remove]");
            if (!button) return;
            details.splice(Number(button.dataset.adjustRemove), 1);
            renderDetailRows();
        });
        form?.addEventListener("submit", async event => {
            event.preventDefault();
            if (!validateContainer(main)) return;
            if (!details.length) { feedback(root, "Agrega al menos un lote al ajuste.", "error"); return; }
            const payload = collectFields(main, fields);
            payload.detalles = details.map(item => ({ loteId: item.loteId, cantidadFisica: item.cantidadFisica, observacion: item.observacion }));
            try {
                const result = await api("/AjusteInventario/Create", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
                feedback(root, result?.message || "Ajuste guardado como borrador.", "success");
                form.reset(); details.splice(0); renderDetailRows();
                await renderFields(main, fields, "ajuste", false, {});
                await renderFields(detail, detailFields, "ajuste-detail", false, {});
                await loadRows();
            } catch (error) { feedback(root, error.message, "error"); }
        });
        body?.addEventListener("click", async event => {
            const button = event.target.closest("[data-adjust-action]");
            if (!button) return;
            const action = button.dataset.adjustAction;
            const endpoint = action === "confirmar" ? "Confirmar" : "Anular";
            const prompt = action === "confirmar"
                ? "¿Confirmas el ajuste? Esta acción actualizará las existencias."
                : "¿Deseas anular este ajuste en borrador?";
            if (!window.confirm(prompt)) return;
            try {
                const result = await api(`/AjusteInventario/${endpoint}?id=${encodeURIComponent(button.dataset.id)}`, { method: "POST" });
                feedback(root, result?.message || "Ajuste actualizado.", "success");
                await loadRows();
            } catch (error) { feedback(root, error.message, "error"); }
        });
        $("[data-fc-new]", root)?.addEventListener("click", () => form?.scrollIntoView({ behavior: "smooth", block: "start" }));
        $("[data-fc-adjustment-clear]", root)?.addEventListener("click", async () => {
            form?.reset(); details.splice(0); renderDetailRows();
            await renderFields(main, fields, "ajuste", false, {});
            await renderFields(detail, detailFields, "ajuste-detail", false, {});
        });
        search?.addEventListener("input", () => { page = 0; loadRows(); });
        $("[data-fc-prev]", root)?.addEventListener("click", () => { if (page > 0) { page--; loadRows(); } });
        $("[data-fc-next]", root)?.addEventListener("click", () => { if ((page + 1) * pageSize < total) { page++; loadRows(); } });
        renderDetailRows();
        await loadRows();
    }

    async function initCash(root) {
        const openForm = $("[data-fc-open-form]", root);
        const closeForm = $("[data-fc-close-form]", root);
        const openFields = $("[data-fc-open-fields]", root);
        const closeFields = $("[data-fc-close-fields]", root);
        const body = $("[data-fc-body]", root);
        const search = $("[data-fc-search]", root);
        const pageSize = 10;
        let page = 0, total = 0, openShiftId = null;
        const openDefs = [
            { name: "sucursalId", label: "Sucursal", type: "select", required: true, source: { url: "/Sucursal/GetActivas", labelKeys: ["codigo", "nombre"] } },
            { name: "montoInicial", label: "Monto inicial", type: "number", required: true, min: 0, step: "0.01" },
            { name: "observaciones", label: "Observaciones", type: "textarea", full: true }
        ];
        const closeDefs = [
            { name: "montoContado", label: "Monto contado", type: "number", required: true, min: 0, step: "0.01" },
            { name: "observaciones", label: "Observaciones", type: "textarea", full: true }
        ];
        await renderFields(openFields, openDefs, "turno-open", false, {});
        await renderFields(closeFields, closeDefs, "turno-close", false, {});
        const columns = [
            { key: "id", label: "ID turno", format: "number" },
            { key: "sucursalNombre", label: "Sucursal" }, { key: "usuarioAperturaNombre", label: "Usuario" },
            { key: "fechaApertura", label: "Apertura (Nicaragua)", format: "datetimeUtc" },
            { key: "fechaCierre", label: "Cierre (Nicaragua)", format: "datetimeUtc" },
            { key: "montoInicial", label: "Monto inicial", format: "currency" }, { key: "montoEsperado", label: "Monto esperado", format: "currency" },
            { key: "montoContado", label: "Monto contado", format: "currency" }, { key: "diferencia", label: "Diferencia", format: "currency" },
            { key: "estado", label: "Estado", format: "status" }
        ];
        setTableHead(root, columns, true);

        async function loadOpenShift() {
            try {
                const shift = await api("/TurnoCaja/MiTurnoAbierto");
                openShiftId = shift.id;
                const idLabel = $("[data-fc-shift-id]", root);
                if (idLabel) idLabel.textContent = `Turno #${shift.id}`;
                $("[data-fc-shift-state]", root).innerHTML = statusPill(shift.estado);
                $("[data-fc-shift-opening]", root).textContent = moneyValue(shift.montoInicial);
                $("[data-fc-shift-branch]", root).textContent = shift.sucursalNombre || "—";
                $("[data-fc-shift-date]", root).textContent = dateTimeManagua(shift.fechaApertura);
                openForm.hidden = true;
                closeForm.hidden = false;
            } catch (error) {
                openShiftId = null;
                const idLabel = $("[data-fc-shift-id]", root);
                if (idLabel) idLabel.textContent = "—";
                $("[data-fc-shift-state]", root).textContent = "Sin turno abierto";
                $("[data-fc-shift-opening]", root).textContent = "—";
                $("[data-fc-shift-branch]", root).textContent = "—";
                $("[data-fc-shift-date]", root).textContent = "—";
                openForm.hidden = false;
                closeForm.hidden = true;
                if (error.message.includes("permiso")) feedback(root, error.message, "error");
            }
        }

        async function loadRows() {
            try {
                const result = await getTableData("/TurnoCaja/GetData", search?.value || "", page * pageSize, pageSize);
                const rows = responseRows(result);
                total = responseCount(result, rows);
                body.innerHTML = rows.length ? rows.map(row => {
                    const action = String(row.estado).toLowerCase() === "abierto"
                        ? `<button type="button" class="fc-action-link" data-cash-close="${escapeHtml(row.id)}">Cerrar</button>` : "—";
                    return `<tr>${columns.map(col => `<td>${displayCell(row, col)}</td>`).join("")}<td>${action}</td></tr>`;
                }).join("") : `<tr><td colspan="${columns.length + 1}" class="fc-empty-cell">No hay turnos registrados.</td></tr>`;
                $("[data-fc-count]", root).textContent = `${total} turno(s)`;
                $("[data-fc-page]", root).textContent = `${page + 1} / ${Math.max(1, Math.ceil(total / pageSize))}`;
                $("[data-fc-prev]", root).disabled = page <= 0;
                $("[data-fc-next]", root).disabled = (page + 1) * pageSize >= total;
            } catch (error) {
                body.innerHTML = `<tr><td colspan="${columns.length + 1}" class="fc-empty-cell">${escapeHtml(error.message)}</td></tr>`;
                feedback(root, error.message, "error");
            }
        }

        async function closeShift(id) {
            const entered = window.prompt("Ingresa el monto contado al cierre:");
            if (entered === null) return;
            const amount = Number(entered);
            if (!Number.isFinite(amount) || amount < 0) { feedback(root, "Introduce un monto válido.", "error"); return; }
            const observations = window.prompt("Observaciones del cierre (opcional):") || "";
            try {
                const result = await api(`/TurnoCaja/Cerrar?id=${encodeURIComponent(id)}`, {
                    method: "POST", headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ montoContado: amount, observaciones: observations })
                });
                feedback(root, result?.message || "Turno cerrado correctamente.", "success");
                await loadOpenShift(); await loadRows();
            } catch (error) { feedback(root, error.message, "error"); }
        }

        openForm?.addEventListener("submit", async event => {
            event.preventDefault();
            if (!openForm.reportValidity()) return;
            const payload = collectFields(openFields, openDefs);
            try {
                const result = await api("/TurnoCaja/Abrir", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
                feedback(root, result?.message || "Turno abierto correctamente.", "success");
                await loadOpenShift(); await loadRows();
            } catch (error) { feedback(root, error.message, "error"); }
        });

        closeForm?.addEventListener("submit", async event => {
            event.preventDefault();
            if (!openShiftId) { feedback(root, "No hay un turno abierto para cerrar.", "error"); return; }
            if (!closeForm.reportValidity()) return;
            const payload = collectFields(closeFields, closeDefs);
            try {
                const result = await api(`/TurnoCaja/Cerrar?id=${encodeURIComponent(openShiftId)}`, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
                feedback(root, result?.message || "Turno cerrado correctamente.", "success");
                closeForm.reset(); await loadOpenShift(); await loadRows();
            } catch (error) { feedback(root, error.message, "error"); }
        });

        body?.addEventListener("click", event => {
            const button = event.target.closest("[data-cash-close]");
            if (button) closeShift(button.dataset.cashClose);
        });
        search?.addEventListener("input", () => { page = 0; loadRows(); });
        $("[data-fc-prev]", root)?.addEventListener("click", () => { if (page > 0) { page--; loadRows(); } });
        $("[data-fc-next]", root)?.addEventListener("click", () => { if ((page + 1) * pageSize < total) { page++; loadRows(); } });
        await loadOpenShift();
        await loadRows();
    }

    async function safeTable(url, length = 100) {
        try { return await getTableData(url, "", 0, length); }
        catch { return null; }
    }

    async function initDashboard(root) {
        const endpoints = {
            medicamentos: "/Medicamento/GetData",
            categorias: "/Categoria/GetData",
            lotes: "/Lote/GetData",
            compras: "/Compra/GetData",
            ventas: "/Venta/GetData"
        };
        const results = await Promise.all(Object.entries(endpoints).map(async ([key, url]) => [key, await safeTable(url, 1000)]));
        const data = Object.fromEntries(results);
        const rows = key => responseRows(data[key]);
        const setKpi = (key, value) => {
            const target = $(`[data-fc-kpi="${key}"]`, root);
            if (target) target.textContent = value;
        };

        setKpi("medicamentos", data.medicamentos ? responseCount(data.medicamentos, rows("medicamentos")) : "—");
        setKpi("categorias", data.categorias ? responseCount(data.categorias, rows("categorias")) : "—");
        const activeLots = rows("lotes").filter(item => item.activo !== false);
        setKpi("lotes", data.lotes ? activeLots.length : "—");

        const today = todayManagua();
        const limit = new Date(`${today}T12:00:00Z`);
        limit.setUTCDate(limit.getUTCDate() + 90);
        const soon = limit.toISOString().slice(0, 10);
        const upcoming = rows("lotes").filter(item => {
            if (item.activo === false || !item.fechaVencimiento) return false;
            const date = String(item.fechaVencimiento).slice(0, 10);
            return date >= today && date <= soon;
        }).sort((a, b) => String(a.fechaVencimiento).localeCompare(String(b.fechaVencimiento)));
        setKpi("vencimientos", data.lotes ? upcoming.length : "—");

        const alertTarget = $("[data-fc-alerts]", root);
        if (alertTarget) {
            alertTarget.innerHTML = upcoming.length
                ? upcoming.slice(0, 6).map(item => `<div class="fc-alert-item"><div class="fc-alert-copy"><strong>${escapeHtml(item.medicamentoNombre || "Medicamento")} · ${escapeHtml(item.numeroLote || "")}</strong><small>${escapeHtml(item.almacenNombre || "Almacén")} · Vence ${dateValue(item.fechaVencimiento)}</small></div><span class="fc-status-pill is-warning">${escapeHtml(item.existencias ?? 0)} uds.</span></div>`).join("")
                : `<div class="fc-empty-state">${data.lotes ? "No hay vencimientos en los próximos 90 días." : "No se pudo consultar el inventario."}</div>`;
        }

        const activities = [
            ...rows("compras").map(item => ({ type: "Compra", name: item.numeroDocumento, detail: item.proveedorNombre, date: item.fechaCompra, amount: item.total })),
            ...rows("ventas").map(item => ({ type: "Venta", name: item.numeroDocumento, detail: item.clienteNombre, date: item.fechaVenta, amount: item.total }))
        ].filter(item => item.date).sort((a, b) => new Date(b.date) - new Date(a.date)).slice(0, 6);

        const activityTarget = $("[data-fc-activity]", root);
        if (activityTarget) {
            activityTarget.innerHTML = activities.length
                ? activities.map(item => `<div class="fc-activity-item"><div class="fc-activity-copy"><strong>${escapeHtml(item.type)} · ${escapeHtml(item.name || "Documento")}</strong><small>${escapeHtml(item.detail || "Sin detalle")} · ${dateValue(item.date)}</small></div><span class="fc-activity-amount">${escapeHtml(moneyValue(item.amount))}</span></div>`).join("")
                : `<div class="fc-empty-state">No hay actividad disponible o el usuario no tiene permisos para consultar estos módulos.</div>`;
        }
    }

    async function initAdminPermissions(root) {
        const body = $("[data-admin-permission-body]", root);
        const form = $("[data-admin-permission-form]", root);
        const search = $("[data-admin-permission-search]", root);
        const feedbackTarget = $("[data-admin-permission-feedback]", root);
        const canCreate = root.dataset.canCreate === "true";
        const canUpdate = root.dataset.canUpdate === "true";
        const canDelete = root.dataset.canDelete === "true";
        let rows = [];

        const showMessage = (message, type = "") => {
            if (!feedbackTarget) return;
            feedbackTarget.textContent = message;
            feedbackTarget.className = `fc-feedback${type ? ` is-${type}` : ""}`;
        };

        async function load() {
            try {
                const result = await getTableData("/Admin/GetPermissionsData", search?.value || "", 0, 500);
                rows = responseRows(result);
                body.innerHTML = rows.length ? rows.map(item => { const actions = `${canUpdate ? `<button type="button" class="fc-action-link" data-permission-edit="${escapeHtml(item.id)}">Editar</button>` : ""}${canDelete ? `<button type="button" class="fc-action-link fc-action-danger" data-permission-delete="${escapeHtml(item.id)}">Eliminar</button>` : ""}`; return `<tr><td>${escapeHtml(item.id)}</td><td class="fc-cell-strong">${escapeHtml(item.name)}</td><td>${escapeHtml(item.description || "—")}</td><td>${actions || "—"}</td></tr>`; }).join("") : `<tr><td colspan="4" class="fc-empty-cell">No hay permisos registrados.</td></tr>`;
                $("[data-admin-permission-count]", root).textContent = `${responseCount(result, rows)} permiso(s)`;
            } catch (error) {
                body.innerHTML = `<tr><td colspan="4" class="fc-empty-cell">${escapeHtml(error.message)}</td></tr>`;
                showMessage(error.message, "error");
            }
        }

        form?.addEventListener("submit", async event => {
            event.preventDefault();
            if (!form.reportValidity()) return;
            const id = form.elements.namedItem("Id").value;
            if (!id && !canCreate) { showMessage("No tienes permiso para crear permisos.", "error"); return; }
            if (id && !canUpdate) { showMessage("No tienes permiso para actualizar permisos.", "error"); return; }
            const payload = { id: id ? Number(id) : null, name: form.elements.namedItem("Name").value.trim(), descripcion: form.elements.namedItem("Descripcion").value.trim() || null };
            const endpoint = id ? "/Admin/UpdatePermission" : "/Admin/CreatePermission";
            try {
                const result = await api(endpoint, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(payload) });
                showMessage(result?.message || "Permiso guardado.", "success");
                form.reset(); form.elements.namedItem("Id").value = "";
                await load();
            } catch (error) { showMessage(error.message, "error"); }
        });

        body?.addEventListener("click", async event => {
            const edit = event.target.closest("[data-permission-edit]");
            const remove = event.target.closest("[data-permission-delete]");
            if (edit) {
                try {
                    const item = await api(`/Admin/GetPermissionById?id=${encodeURIComponent(edit.dataset.permissionEdit)}`);
                    form.elements.namedItem("Id").value = item.id;
                    form.elements.namedItem("Name").value = item.name || "";
                    form.elements.namedItem("Descripcion").value = item.description || item.descripcion || "";
                    form.scrollIntoView({ behavior: "smooth", block: "center" });
                } catch (error) { showMessage(error.message, "error"); }
            }
            if (remove) {
                if (!window.confirm("¿Deseas eliminar este permiso?")) return;
                try {
                    const result = await api(`/Admin/DeletePermission?id=${encodeURIComponent(remove.dataset.permissionDelete)}`, { method: "POST" });
                    showMessage(result?.message || "Permiso eliminado.", "success");
                    await load();
                } catch (error) { showMessage(error.message, "error"); }
            }
        });
        $("[data-admin-permission-clear]", root)?.addEventListener("click", () => form?.reset());
        search?.addEventListener("input", load);
        await load();
    }


    async function initAdminRoleDetails(root) {
        const feedbackTarget = $("[data-admin-role-feedback]", root);
        const show = (message, type = "") => {
            if (!feedbackTarget) return;
            feedbackTarget.textContent = message;
            feedbackTarget.className = `fc-feedback${type ? ` is-${type}` : ""}`;
        };
        root.addEventListener("submit", async event => {
            const assignForm = event.target.closest("[data-admin-role-assign]");
            const removeForm = event.target.closest("[data-admin-role-remove]");
            const form = assignForm || removeForm;
            if (!form) return;
            event.preventDefault();
            const body = new URLSearchParams(new FormData(form));
            try {
                const result = await api(form.action, {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
                    body
                });
                show(result?.message || "Permisos del rol actualizados.", "success");
                window.location.reload();
            } catch (error) { show(error.message, "error"); }
        });
    }

    async function initAdminUserPermissions(root) {
        const userId = root.dataset.userId;
        const feedbackTarget = $("[data-admin-user-feedback]", root);
        const show = (message, type = "") => {
            if (!feedbackTarget) return;
            feedbackTarget.textContent = message;
            feedbackTarget.className = `fc-feedback${type ? ` is-${type}` : ""}`;
        };

        $("[data-admin-user-role-form]", root)?.addEventListener("submit", async event => {
            event.preventDefault();
            const form = event.currentTarget;
            const rolId = Number(form.elements.namedItem("RolId").value);
            try {
                const result = await api(`/Admin/UpdateRole?userId=${encodeURIComponent(userId)}`, {
                    method: "POST", headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ rolId })
                });
                show(result?.message || "Rol actualizado correctamente.", "success");
            } catch (error) { show(error.message, "error"); }
        });

        $("[data-admin-user-permission-form]", root)?.addEventListener("submit", async event => {
            event.preventDefault();
            const form = event.currentTarget;
            const payload = {
                permissionId: Number(form.elements.namedItem("PermissionId").value),
                isGranted: form.elements.namedItem("IsGranted").value === "true"
            };
            try {
                const result = await api(`/Admin/AssingPermissionToUser?userId=${encodeURIComponent(userId)}`, {
                    method: "POST", headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                });
                show(result?.message || "Excepción guardada.", "success");
                window.location.reload();
            } catch (error) { show(error.message, "error"); }
        });

        root.addEventListener("submit", async event => {
            const form = event.target.closest("[data-admin-user-remove]");
            if (!form) return;
            event.preventDefault();
            try {
                const result = await api(form.action, {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded; charset=UTF-8" },
                    body: new URLSearchParams(new FormData(form))
                });
                show(result?.message || "Excepción retirada.", "success");
                window.location.reload();
            } catch (error) { show(error.message, "error"); }
        });
    }

    // Menú adaptable
    document.addEventListener("DOMContentLoaded", () => {
        const toggle = $("#sidebarToggle");
        const backdrop = $("#sidebarBackdrop");
        const closeSidebar = () => {
            document.body.classList.remove("sidebar-open");
            toggle?.setAttribute("aria-expanded", "false");
        };
        toggle?.addEventListener("click", () => {
            const opened = document.body.classList.toggle("sidebar-open");
            toggle.setAttribute("aria-expanded", String(opened));
        });
        backdrop?.addEventListener("click", closeSidebar);
        window.addEventListener("resize", () => { if (window.innerWidth >= 992) closeSidebar(); });

        $$("[data-fc-module]").forEach(initGenericModule);
        $$("[data-fc-transaction]").forEach(initTransaction);
        $$("[data-fc-adjustment]").forEach(initAdjustment);
        $$("[data-fc-cash]").forEach(initCash);
        $$("[data-fc-dashboard]").forEach(initDashboard);
        $$("[data-fc-admin-permissions]").forEach(initAdminPermissions);
        $$("[data-fc-user-permissions]").forEach(initAdminUserPermissions);
        $$("[data-fc-role-details]").forEach(initAdminRoleDetails);
    });
})();

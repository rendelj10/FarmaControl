/* FarmaControl - consultas de Reportes (solo lectura) */
(() => {
    "use strict";
    const root = document.querySelector("[data-fcr-root]");
    if (!root) return;
    const tabs = Array.from(root.querySelectorAll("[data-fcr-type]"));
    if (!tabs.length) return; // usuario sin acceso
    const $ = (s) => root.querySelector(s);
    const head = $("[data-fcr-head]");
    const body = $("[data-fcr-body]");
    const form = $("[data-fcr-form]");
    const exportButton = $("[data-fcr-export]");
    const totalElement = $("[data-fcr-total]");
    const countElement = $("[data-fcr-count]");
    const noteElement = $("[data-fcr-note]");
    const feedbackElement = $("[data-fcr-feedback]");
    const titleElement = $("[data-fcr-title]");
    const subtitleElement = $("[data-fcr-subtitle]");
    const totalLabelElement = $("[data-fcr-total-label]");
    const formats = {
        ventas: {
            title: "Ventas por período", subtitle: "Total: únicamente ventas confirmadas.",
            totalLabel: "Total confirmado", url: "/Reporte/Ventas", amount: true,
            columns: [ ["fecha", "Fecha", "date"], ["referencia", "N.º de venta"],
                ["relacionado", "Cliente"], ["descripcion", "Almacén"],
                ["estado", "Estado", "status"], ["monto", "Total (C$)", "money"] ]
        },
        compras: {
            title: "Compras por período", subtitle: "Total: únicamente compras recibidas.",
            totalLabel: "Total recibido", url: "/Reporte/Compras", amount: true,
            columns: [ ["fecha", "Fecha", "date"], ["referencia", "N.º de compra"],
                ["relacionado", "Proveedor"], ["descripcion", "Almacén"],
                ["estado", "Estado", "status"], ["monto", "Total (C$)", "money"] ]
        },
        pagos: {
            title: "Pagos por período", subtitle: "Total: únicamente pagos registrados.",
            totalLabel: "Total pagado", url: "/Reporte/Pagos", amount: true,
            columns: [ ["fecha", "Fecha y hora (Nicaragua)", "utcDateTime"], ["referencia", "Referencia"],
                ["relacionado", "Método"], ["descripcion", "Origen"],
                ["estado", "Estado", "status"], ["monto", "Monto (C$)", "money"] ]
        },
        lotes: {
            title: "Existencias y vencimientos", subtitle: "Lotes organizados por fecha de vencimiento.",
            totalLabel: "Unidades disponibles (lotes activos)", url: "/Reporte/Lotes", amount: false,
            columns: [ ["referencia", "N.º de lote"], ["relacionado", "Medicamento"],
                ["descripcion", "Almacén"], ["cantidad", "Existencias", "int"],
                ["vencimiento", "Vencimiento", "date"], ["estado", "Estado", "status"] ]
        }
    };
    let selected = tabs[0].dataset.fcrType;
    let rows = [];
    let current = formats[selected];
    const money = value => value == null ? "—" : "C$ " + Number(value).toLocaleString("es-NI", {minimumFractionDigits:2,maximumFractionDigits:2});
    const date = value => {
        if (!value) return "—";
        const match = String(value).match(/^(\d{4})-(\d{2})-(\d{2})/);
        return match ? `${match[3]}/${match[2]}/${match[1]}` : String(value);
    };
    const localTime = value => {
        if (!value) return "—";
        const raw = String(value).trim();
        const iso = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(raw) ? raw : raw + "Z";
        const d = new Date(iso);
        return Number.isNaN(d.getTime()) ? raw : new Intl.DateTimeFormat("es-NI", {
            timeZone: "America/Managua", day:"2-digit", month:"2-digit", year:"numeric",
            hour:"numeric", minute:"2-digit", hour12:true
        }).format(d);
    };
    const cellText = (row, col) => {
        const [key,,format] = col; const value = row?.[key];
        if (format === "money") return money(value);
        if (format === "date") return date(value);
        if (format === "utcDateTime") return localTime(value);
        if (format === "int") return value == null ? "—" : Number(value).toLocaleString("es-NI");
        return value == null || value === "" ? "—" : String(value);
    };
    const setFeedback = message => { feedbackElement.textContent = message || ""; };
    const csvCell = value => {
        let str = String(value ?? "");
        if (/^\s*[=+@\-]/.test(str)) str = "'" + str; // evita fórmula en hoja de cálculo
        return '"' + str.replace(/"/g, '""') + '"';
    };
    async function load() {
        current = formats[selected]; if (!current) return;
        exportButton.disabled = true; rows = []; setFeedback(""); noteElement.textContent = "";
        titleElement.textContent = current.title;
        subtitleElement.textContent = current.subtitle;
        totalLabelElement.textContent = current.totalLabel;
        countElement.textContent = "—"; totalElement.textContent = "—";
        const noDates = selected === "lotes";
        root.querySelectorAll("[data-fcr-dates]").forEach(el => { el.hidden = noDates; });
        head.replaceChildren();
        current.columns.forEach(col => { const th = document.createElement("th"); th.textContent = col[1]; head.append(th); });
        body.replaceChildren();
        const placeholder = document.createElement("tr"); const td = document.createElement("td");
        td.colSpan = current.columns.length; td.textContent = "Consultando información..."; placeholder.append(td); body.append(placeholder);
        const params = new URLSearchParams();
        if (!noDates) {
            const start = form.elements.namedItem("desde")?.value;
            const end = form.elements.namedItem("hasta")?.value;
            if (start) params.set("desde", start);
            if (end) params.set("hasta", end);
            if (start && end && start > end) { setFeedback("La fecha inicial no puede ser posterior a la final."); return; }
        }
        try {
            const response = await fetch(current.url + (params.size ? "?"+params.toString() : ""), {
                credentials: "same-origin", headers: { "Accept": "application/json", "X-Requested-With": "XMLHttpRequest" }
            });
            if (!response.ok) {
                if (response.status === 401 || response.redirected) throw new Error("La sesión venció. Volvé a iniciar sesión.");
                if (response.status === 403) throw new Error("Tu usuario no tiene permiso para consultar este reporte.");
                throw new Error(`No se pudo consultar el reporte (HTTP ${response.status}).`);
            }
            const data = await response.json();
            if (!Array.isArray(data.filas)) throw new Error("El servidor devolvió un formato de reporte inesperado.");
            rows = data.filas;
            countElement.textContent = Number(data.totalRegistros ?? rows.length).toLocaleString("es-NI");
            totalElement.textContent = current.amount ? money(data.totalMonetario ?? 0) : Number(data.totalCantidad ?? 0).toLocaleString("es-NI");
            body.replaceChildren();
            if (!rows.length) {
                const tr = document.createElement("tr"); const cell = document.createElement("td");
                cell.colSpan = current.columns.length; cell.textContent = "No se encontraron registros."; tr.append(cell); body.append(tr);
            } else {
                rows.forEach(row => {
                    const tr = document.createElement("tr");
                    current.columns.forEach(col => {
                        const td = document.createElement("td");
                        if (col[2] === "status") {
                            const badge = document.createElement("span");
                            const state = String(row?.[col[0]] ?? "");
                            badge.textContent = state || "—";
                            badge.className = "fcr-status" +
                                (/^(Confirmada|Recibida|Registrado|Activo)$/.test(state) ? " fcr-status-confirmed" :
                                 (/^Borrador$/.test(state) ? " fcr-status-draft" :
                                  (/^(Anulada|Anulado|Inactivo)$/.test(state) ? " fcr-status-cancelled" : "")));
                            td.append(badge);
                        } else td.textContent = cellText(row, col);
                        tr.append(td);
                    });
                    body.append(tr);
                });
            }
            exportButton.disabled = !rows.length;
            noteElement.textContent = data.truncado ?
                `Mostrando ${rows.length} de ${data.totalRegistros} registros; el CSV solo incluye las filas mostradas. Acotá las fechas.` :
                "Consulta de solo lectura. Podés exportar las filas mostradas a CSV.";
        } catch (err) {
            rows = []; body.replaceChildren(); exportButton.disabled = true;
            setFeedback(err?.message || "Error inesperado al consultar.");
        }
    }
    tabs.forEach(tab => tab.addEventListener("click", () => {
        selected = tab.dataset.fcrType;
        tabs.forEach(t => t.setAttribute("aria-pressed", String(t === tab)));
        load();
    }));
    form.addEventListener("submit", ev => { ev.preventDefault(); load(); });
    exportButton.addEventListener("click", () => {
        if (!rows.length || !current) return;
        const csv = [current.columns.map(col => csvCell(col[1])).join(";"),
            ...rows.map(row => current.columns.map(col => csvCell(cellText(row, col))).join(";"))].join("\r\n");
        const blob = new Blob(["\uFEFF", csv], {type:"text/csv;charset=utf-8"});
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a"); a.href = url;
        const dia = Object.fromEntries(new Intl.DateTimeFormat("en-US", {
            timeZone: "America/Managua", day: "2-digit", month: "2-digit", year: "numeric"
        }).formatToParts(new Date()).map(p => [p.type, p.value]));
        a.download = `FarmaControl_${selected}_${dia.year}-${dia.month}-${dia.day}.csv`;
        document.body.append(a); a.click(); a.remove(); URL.revokeObjectURL(url);
    });
    tabs[0].setAttribute("aria-pressed", "true");
    load();
})();

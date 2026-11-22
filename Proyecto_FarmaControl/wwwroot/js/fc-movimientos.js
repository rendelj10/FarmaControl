/* FarmaControl: movimientos por compra, venta y ajuste. Solo lectura. */
(() => {
    "use strict";
    const root = document.querySelector("[data-fc-movement-root]");
    if (!root) return;
    const $ = selector => root.querySelector(selector);
    const form = $("[data-fc-report-form]");
    const tbody = $("[data-fc-report-body]");
    const count = $("[data-fc-report-count]");
    const total = $("[data-fc-report-total]");
    const notice = $("[data-fc-report-notice]");
    const feedback = $("[data-fc-report-feedback]");
    const exportBtn = $("[data-fc-report-export]");
    if (!form || !tbody || !count || !total || !notice || !feedback || !exportBtn) return;

    const formatter = new Intl.DateTimeFormat("es-NI", {
        timeZone: "America/Managua", day: "2-digit", month: "2-digit", year: "numeric",
        hour: "numeric", minute: "2-digit", hour12: true
    });
    // Los movimientos creados por el backend se almacenan en UTC; SQL Server omite el sufijo Z.
    const fechaLocal = value => {
        if (!value) return "—";
        const raw = String(value).trim();
        const iso = /(?:Z|[+-]\d{2}:?\d{2})$/i.test(raw) ? raw : raw + "Z";
        const d = new Date(iso);
        return Number.isNaN(d.getTime()) ? raw : formatter.format(d);
    };
    const tipoNombre = value => ({
        SalidaVenta: "Salida por venta", EntradaCompra: "Entrada por compra",
        AjusteSalida: "Salida por ajuste", AjusteEntrada: "Entrada por ajuste"
    })[String(value)] || String(value || "—").replace(/([a-z])([A-Z])/g, "$1 $2");
    const operacion = row => row.ventaId != null ? `Venta #${row.ventaId}` :
        row.compraId != null ? `Compra #${row.compraId}` :
        row.ajusteInventarioId != null ? `Ajuste #${row.ajusteInventarioId}` :
        `Movimiento #${row.id}`;
    const numeros = value => Number(value || 0).toLocaleString("es-NI");
    const columnas = [
        ["fecha", "Fecha y hora (Nicaragua)", fechaLocal],
        ["relacionado", "Tipo", tipoNombre],
        ["__operacion", "Operación", (_, row) => operacion(row)],
        ["referencia", "Documento", v => v || "—"],
        ["descripcion", "Origen → destino", v => v || "—"],
        ["estado", "Estado", v => v || "—"],
        ["cantidad", "Unidades", numeros]
    ];
    let rows = [], shown = [], truncado = false, cantidadTotal = 0;
    const mensaje = (text, error = false) => {
        feedback.textContent = text || "";
        feedback.className = error ? "fc-feedback is-error" : "fc-feedback";
    };
    const placeholder = text => {
        tbody.replaceChildren();
        const tr = document.createElement("tr");
        const td = document.createElement("td");
        td.colSpan = columnas.length;
        td.className = "fc-empty-cell";
        td.textContent = text;
        tr.append(td); tbody.append(tr);
    };
    const valor = (row, col) => col[2](row?.[col[0]], row);
    const coincide = (row, search) => {
        if (!search) return true;
        const text = [operacion(row), row.referencia, row.relacionado, tipoNombre(row.relacionado),
            row.descripcion, row.estado, row.id].join(" ").toLocaleLowerCase("es-NI");
        return text.includes(search);
    };
    const pintar = () => {
        const busqueda = String(form.elements.namedItem("buscar")?.value || "").trim().toLocaleLowerCase("es-NI");
        shown = rows.filter(row => coincide(row, busqueda));
        count.textContent = numeros(shown.length);
        total.textContent = numeros(shown.reduce((acc, r) => acc + Number(r.cantidad || 0), 0));
        tbody.replaceChildren();
        if (!shown.length) {
            placeholder(rows.length ? "No hay coincidencias. Revisá la búsqueda." : "No se encontraron movimientos.");
        } else {
            for (const row of shown) {
                const tr = document.createElement("tr");
                columnas.forEach((col, i) => {
                    const td = document.createElement("td");
                    td.textContent = valor(row, col);
                    if (i === 2 || i === 3) td.className = "fcm-emph";
                    if (i === 1) {
                        const span = document.createElement("span");
                        span.className = "fcm-type"; span.textContent = td.textContent;
                        td.replaceChildren(span);
                    }
                    tr.append(td);
                });
                tbody.append(tr);
            }
        }
        notice.textContent = truncado
            ? `El servidor devolvió un máximo de ${rows.length} movimientos. La búsqueda y el CSV incluyen solo los datos cargados; acotá el período para encontrar otros.`
            : `Se muestran ${shown.length} movimiento(s). Podés buscar por número de venta (ej.: 11) o documento.`;
        exportBtn.disabled = !shown.length;
    };
    async function load() {
        rows = []; shown = []; truncado = false; cantidadTotal = 0;
        exportBtn.disabled = true; count.textContent = "—"; total.textContent = "—";
        notice.textContent = ""; mensaje("");
        const desde = form.elements.namedItem("desde")?.value || "";
        const hasta = form.elements.namedItem("hasta")?.value || "";
        if (desde && hasta && desde > hasta) {
            placeholder("Revisá el período seleccionado.");
            mensaje("La fecha inicial no puede ser posterior a la final.", true); return;
        }
        placeholder("Consultando movimientos...");
        const params = new URLSearchParams();
        if (desde) params.set("desde", desde);
        if (hasta) params.set("hasta", hasta);
        try {
            const response = await fetch("/Movimiento/Consultar" + (params.size ? "?" + params : ""), {
                credentials: "same-origin", headers: { Accept: "application/json", "X-Requested-With": "XMLHttpRequest" }
            });
            if (response.redirected || response.status === 401) throw new Error("Tu sesión venció. Volvé a iniciar sesión.");
            if (response.status === 403) throw new Error("Tu usuario no tiene permiso para consultar movimientos.");
            if (!response.ok) throw new Error(`No se pudo consultar movimientos (HTTP ${response.status}).`);
            const data = await response.json();
            if (!Array.isArray(data.filas)) throw new Error("El servidor devolvió datos inesperados.");
            rows = data.filas;
            truncado = !!data.truncado;
            cantidadTotal = Number(data.totalCantidad || 0);
            pintar();
        } catch (e) {
            placeholder("No se pudo cargar el historial."); mensaje(e?.message || "Error inesperado.", true);
        }
    }
    const csvCell = value => {
        let s = String(value ?? "");
        if (/^\s*[=+@\-]/.test(s)) s = "'" + s;
        return '"' + s.replace(/"/g, '""') + '"';
    };
    form.addEventListener("submit", ev => { ev.preventDefault(); load(); });
    form.elements.namedItem("buscar")?.addEventListener("input", pintar);
    exportBtn.addEventListener("click", () => {
        if (!shown.length) return;
        const content = [columnas.map(c => csvCell(c[1])).join(";"),
            ...shown.map(r => columnas.map(c => csvCell(valor(r,c))).join(";"))].join("\r\n");
        const blob = new Blob(["\uFEFF", content], { type: "text/csv;charset=utf-8" });
        const url = URL.createObjectURL(blob), a = document.createElement("a");
        a.href = url; a.download = `FarmaControl_Movimientos_${new Intl.DateTimeFormat("en-CA", {timeZone:"America/Managua",year:"numeric",month:"2-digit",day:"2-digit"}).format(new Date())}.csv`;
        document.body.append(a); a.click(); a.remove(); URL.revokeObjectURL(url);
    });
    load();
})();

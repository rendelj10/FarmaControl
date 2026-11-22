/* FarmaControl: consulta de movimientos, solo lectura, independiente de site.js */
(() => {
    'use strict';
    const root = document.querySelector('[data-fc-movement-root]');
    if (!root) return;
    const query = selector => root.querySelector(selector);
    const form = query('[data-fc-report-form]');
    const tbody = query('[data-fc-report-body]');
    const count = query('[data-fc-report-count]');
    const total = query('[data-fc-report-total]');
    const notice = query('[data-fc-report-notice]');
    const feedback = query('[data-fc-report-feedback]');
    const exportBtn = query('[data-fc-report-export]');
    if (!form || !tbody || !count || !total || !notice || !feedback || !exportBtn) return;

    const cols = [
        ['fecha', 'Fecha', 'date'], ['relacionado', 'Tipo'], ['referencia', 'Referencia'],
        ['descripcion', 'Origen → destino'], ['estado', 'Estado'], ['cantidad', 'Unidades', 'number']
    ];
    let rows = [];
    const number = n => Number(n || 0).toLocaleString('es-NI');
    const date = value => {
        if (!value) return '—';
        const match = String(value).match(/^(\d{4})-(\d{2})-(\d{2})/);
        return match ? `${match[3]}/${match[2]}/${match[1]}` : String(value);
    };
    const formatted = (row, c) => {
        const v = row?.[c[0]];
        if (v == null || v === '') return '—';
        return c[2] === 'date' ? date(v) : c[2] === 'number' ? number(v) : String(v);
    };
    const setMessage = (s, error = false) => {
        feedback.textContent = s || '';
        feedback.className = error ? 'fc-feedback is-error' : 'fc-feedback';
    };
    const placeholder = s => {
        tbody.replaceChildren();
        const tr = document.createElement('tr');
        const td = document.createElement('td');
        td.colSpan = cols.length;
        td.className = 'fc-empty-cell';
        td.textContent = s;
        tr.append(td);
        tbody.append(tr);
    };
    async function load() {
        rows = [];
        exportBtn.disabled = true;
        count.textContent = '—';
        total.textContent = '—';
        notice.textContent = '';
        setMessage('');
        const desde = form.elements.namedItem('desde')?.value || '';
        const hasta = form.elements.namedItem('hasta')?.value || '';
        if (desde && hasta && desde > hasta) {
            placeholder('Revisá el período seleccionado.');
            setMessage('La fecha inicial no puede ser posterior a la final.', true);
            return;
        }
        placeholder('Consultando movimientos...');
        const params = new URLSearchParams();
        if (desde) params.set('desde', desde);
        if (hasta) params.set('hasta', hasta);
        const url = '/Movimiento/Consultar' + (params.toString() ? '?' + params.toString() : '');
        try {
            const response = await fetch(url, {credentials:'same-origin',headers:{Accept:'application/json','X-Requested-With':'XMLHttpRequest'}});
            if (response.redirected || response.status === 401) throw new Error('Tu sesión venció. Volvé a iniciar sesión.');
            if (response.status === 403) throw new Error('No tenés permiso para consultar movimientos.');
            if (!response.ok) throw new Error('Error al consultar movimientos (HTTP ' + response.status + ').');
            const data = await response.json();
            if (!Array.isArray(data.filas)) throw new Error('El servidor devolvió datos inesperados.');
            rows = data.filas;
            count.textContent = number(data.totalRegistros ?? rows.length);
            total.textContent = number(data.totalCantidad ?? 0);
            tbody.replaceChildren();
            if (rows.length === 0) placeholder('No se encontraron movimientos.');
            for (const r of rows) {
                const tr = document.createElement('tr');
                for (const c of cols) {
                    const td = document.createElement('td');
                    td.textContent = formatted(r,c);
                    tr.append(td);
                }
                tbody.append(tr);
            }
            notice.textContent = data.truncado
                ? `Se muestran ${rows.length} de ${data.totalRegistros} registros. El CSV incluye solo las filas visibles; acotá las fechas.`
                : 'Consulta de solo lectura. Las existencias no se modifican.';
            exportBtn.disabled = rows.length === 0;
        } catch (error) {
            rows = [];
            placeholder('No se pudo cargar el historial.');
            setMessage(error?.message || 'Error inesperado.', true);
        }
    }
    const csv = value => {
        let str = String(value ?? '');
        if (/^\s*[=+@\-]/.test(str)) str = "'" + str;
        return '"' + str.replace(/"/g,'""') + '"';
    };
    form.addEventListener('submit', event => { event.preventDefault(); load(); });
    exportBtn.addEventListener('click', () => {
        if (!rows.length) return;
        const csvText = [cols.map(c => csv(c[1])).join(';'),
            ...rows.map(r => cols.map(c => csv(formatted(r,c))).join(';'))].join('\r\n');
        const blob = new Blob(['\uFEFF',csvText],{type:'text/csv;charset=utf-8'});
        const href = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = href;
        a.download = `FarmaControl_Movimientos_${new Date().toISOString().slice(0,10)}.csv`;
        document.body.append(a);
        a.click(); a.remove();
        URL.revokeObjectURL(href);
    });
    load();
})();

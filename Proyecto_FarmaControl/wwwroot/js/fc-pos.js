/* FarmaControl POS de mostrador. Reutiliza los controladores MVC existentes.
   No altera el stock en el navegador: el servidor valida y descuenta al CONFIRMAR. */
(() => {
    "use strict";
    const root = document.querySelector("[data-fc-pos]");
    if (!root) return;
    const $ = selector => root.querySelector(selector);
    const canCreate = root.dataset.canCreate === "true";
    const canConfirm = root.dataset.canConfirm === "true";
    const canCancel = root.dataset.canCancel === "true";
    const canPay = root.dataset.canPay === "true";
    const state = { clients: [], warehouses: [], selectedMedicine: null, lots: [], cart: new Map(), page: 0, count: 0, busy: false, medicineSeq: 0, searchTimer: null };
    const pesos = value => `C$ ${Number(value || 0).toLocaleString("es-NI", {minimumFractionDigits: 2, maximumFractionDigits: 2})}`;
    const escape = value => String(value ?? "").replace(/[&<>"']/g, ch => ({"&":"&amp;", "<":"&lt;", ">":"&gt;", '"':"&quot;", "'":"&#39;"}[ch]));
    const nowManagua = () => Object.fromEntries(new Intl.DateTimeFormat("en-GB", {
        timeZone: "America/Managua", year: "numeric", month: "2-digit", day: "2-digit",
        hour: "2-digit", minute: "2-digit", second: "2-digit", hourCycle: "h23"
    }).formatToParts(new Date()).map(p => [p.type, p.value]));
    const today = () => { const p = nowManagua(); return `${p.year}-${p.month}-${p.day}`; };
    const readDate = value => String(value || "").slice(0, 10);
    const showDate = value => { const d = readDate(value); return /^\d{4}-\d{2}-\d{2}$/.test(d) ? `${d.slice(8,10)}/${d.slice(5,7)}/${d.slice(0,4)}` : "—"; };
    const currentWarehouse = () => Number($("#pos-almacen")?.value || 0);

    function notify(message, severity = "success") {
        const area = $("[data-pos-message]");
        if (!area) return;
        area.textContent = message;
        area.hidden = !message;
        area.classList.toggle("is-error", severity === "error");
        area.classList.toggle("is-warning", severity === "warning");
        if (message) area.scrollIntoView({block:"nearest",behavior:"smooth"});
    }
    function makeDocument() {
        const d = nowManagua();
        const h = `${d.hour}${d.minute}${d.second}`;
        const random = new Uint32Array(1);
        if (window.crypto?.getRandomValues) window.crypto.getRandomValues(random);
        else random[0] = Math.floor(Math.random() * 999999);
        return `POS-${today().replace(/-/g, "")}-${h}-${String(random[0] % 1000000).padStart(6,"0")}`;
    }
    async function request(url, options = {}) {
        const headers = new Headers(options.headers || {});
        headers.set("Accept", "application/json");
        headers.set("X-Requested-With", "XMLHttpRequest");
        if ((options.method || "GET").toUpperCase() !== "GET") {
            const csrf = document.querySelector("#fc-antiforgery-token input[name='__RequestVerificationToken']")?.value
                || document.querySelector("input[name='__RequestVerificationToken']")?.value;
            if (csrf) headers.set("RequestVerificationToken", csrf);
        }
        const response = await fetch(url, {credentials:"same-origin",...options,headers});
        const type = response.headers.get("content-type") || "";
        let payload = {};
        if (type.includes("application/json")) payload = await response.json();
        if (response.status === 401) { window.location.assign("/Auth/Login"); throw new Error("Sesión expirada. Iniciá sesión nuevamente."); }
        if (response.status === 403) throw new Error("Tu rol no tiene permiso para realizar esta acción.");
        if (!response.ok) throw new Error(payload?.message || `No se pudo completar la solicitud (${response.status}).`);
        return payload;
    }
    function tableBody(search, offset, length) {
        const data = new URLSearchParams();
        data.set("Draw", "1"); data.set("Start", String(offset)); data.set("Length", String(length));
        data.set("Search.Value", search || ""); data.set("Search.Regex", "false");
        return data;
    }
    const tableData = (url, search = "", offset = 0, length = 100) => request(url, {
        method:"POST", headers:{"Content-Type":"application/x-www-form-urlencoded; charset=UTF-8"},
        body:tableBody(search,offset,length)
    });
    const rows = response => response?.data || response?.Data || [];
    const filtered = response => Number(response?.recordsFiltered ?? response?.RecordsFiltered ?? rows(response).length);
    async function allPages(url, search, maxRecords = 2000) {
        const result = [];
        for (let offset = 0; offset < maxRecords; offset += 100) {
            const response = await tableData(url,search,offset,100);
            const batch = rows(response);
            result.push(...batch);
            if (result.length >= filtered(response) || batch.length === 0) return result;
        }
        throw new Error("Hay demasiados lotes coincidentes para una búsqueda segura. Refiná el medicamento o consultá Inventario.");
    }
    function fillSelect(element, data, title, labeler) {
        if (!element) return;
        element.innerHTML = `<option value="">${escape(title)}</option>` + data.map(r => `<option value="${Number(r.id)}">${escape(labeler(r))}</option>`).join("");
    }
    function availableLots(medicineId) {
        const warehouseId = currentWarehouse();
        const todayDate = today();
        return state.lots.filter(l => Number(l.medicamentoId) === Number(medicineId)
            && Number(l.almacenId) === warehouseId && l.activo !== false
            && Number(l.existencias) > 0 && readDate(l.fechaVencimiento) > todayDate)
            .sort((a,b) => readDate(a.fechaVencimiento).localeCompare(readDate(b.fechaVencimiento)) || Number(a.id)-Number(b.id));
    }
    function totalStock(medicineId) { return availableLots(medicineId).reduce((sum,l) => sum + Number(l.existencias || 0),0); }
    function cartQuantity(medicineId) { return state.cart.get(Number(medicineId))?.quantity || 0; }
    function allocations(medicineId, desired) {
        let remaining = desired;
        const result = [];
        for (const lot of availableLots(medicineId)) {
            const quantity = Math.min(remaining,Number(lot.existencias));
            if (quantity > 0) result.push({medicamentoId:Number(medicineId),loteId:Number(lot.id),cantidad:quantity});
            remaining -= quantity;
            if (remaining === 0) break;
        }
        return remaining === 0 ? result : null;
    }
    function renderCart() {
        const items = Array.from(state.cart.values());
        const body = $("[data-pos-cart]");
        if (body) body.innerHTML = items.length ? items.map(item => {
            const subtotal = item.quantity * Number(item.price);
            return `<tr><td><div class="fc-pos-cart-title">${escape(item.name)}</div><div class="fc-pos-cart-sub">${escape(item.code)}</div></td>`
                + `<td><div class="fc-pos-qty-controls"><button class="fc-pos-small-btn" type="button" data-pos-minus="${item.id}" aria-label="Reducir cantidad">−</button><strong>${item.quantity}</strong><button class="fc-pos-small-btn" type="button" data-pos-plus="${item.id}" aria-label="Aumentar cantidad">+</button></div></td>`
                + `<td>${escape(pesos(item.price))}</td><td>${escape(pesos(subtotal))}</td><td><button class="fc-pos-remove" type="button" data-pos-remove="${item.id}">Quitar</button></td></tr>`;
        }).join("") : `<tr><td colspan="5" class="fc-empty-cell">Agregá un medicamento para comenzar.</td></tr>`;
        const count = items.reduce((sum,item) => sum + item.quantity,0);
        const total = items.reduce((sum,item) => sum + item.quantity * Number(item.price),0);
        if ($("[data-pos-items]")) $("[data-pos-items]").textContent = String(count);
        if ($("[data-pos-total]")) $("[data-pos-total]").textContent = pesos(total);
    }
    function renderSelected() {
        const medicine = state.selectedMedicine;
        const info = $("[data-pos-selected]");
        const add = $("[data-pos-add]");
        if (!info || !add) return;
        info.hidden = !medicine;
        add.disabled = !medicine || !currentWarehouse() || state.busy;
        if (!medicine) return;
        $("[data-pos-medicine-name]").textContent = medicine.nombre;
        $("[data-pos-medicine-info]").textContent = `${medicine.codigo || ""} · ${medicine.concentracion || ""}`;
        const stock = totalStock(medicine.id);
        $("[data-pos-stock]").textContent = stock > 0 ? `${stock} unidades disponibles en el almacén` : "Sin lotes vigentes con existencia en este almacén";
        $("[data-pos-price]").textContent = pesos(medicine.precioVenta);
        add.disabled = stock <= cartQuantity(medicine.id) || Number(medicine.precioVenta) <= 0 || !currentWarehouse() || state.busy;
    }
    function hideSuggestions() { const target = $("[data-pos-suggestions]"); if(target) {target.hidden = true;target.innerHTML = "";} }
    async function selectMedicine(medicine) {
        state.selectedMedicine = medicine;
        $("#pos-buscar").value = `${medicine.codigo} · ${medicine.nombre}`;
        hideSuggestions();
        renderSelected();
        try {
            const seq = ++state.medicineSeq;
            const lots = await allPages("/Lote/GetData", medicine.nombre, 2000);
            if (seq !== state.medicineSeq) return;
            state.lots = lots.filter(l => Number(l.medicamentoId) === Number(medicine.id));
            renderSelected();
        } catch (error) { state.selectedMedicine = null; state.lots = []; renderSelected(); notify(error.message,"error"); }
    }
    async function findMedicine(query) {
        const target = $("[data-pos-suggestions]");
        if (!target) return;
        const seq = ++state.medicineSeq;
        if (query.trim().length < 2) {hideSuggestions();return;}
        target.hidden = false;
        target.innerHTML = `<div class="fc-pos-suggestion">Buscando medicamentos…</div>`;
        try {
            const result = await tableData("/Medicamento/GetData",query.trim(),0,100);
            if (seq !== state.medicineSeq) return;
            const matches = rows(result).filter(m => m.activo !== false).slice(0,14);
            target.innerHTML = matches.length ? matches.map((med,index) => `<button type="button" class="fc-pos-suggestion" data-pos-result="${index}"><span><strong>${escape(med.nombre)}</strong><small>${escape(med.codigo)} · ${escape(med.concentracion || "")}</small></span><span class="fc-pos-suggestion-price">${escape(pesos(med.precioVenta))}</span></button>`).join("")
                : `<div class="fc-pos-suggestion">No hay medicamentos activos con ese nombre o código.</div>`;
            target._items = matches;
        } catch (error) {
            if (seq !== state.medicineSeq) return;
            target.innerHTML = `<div class="fc-pos-suggestion">${escape(error.message)}</div>`;
        }
    }
    function addMedicine() {
        const med = state.selectedMedicine;
        if (!med || state.busy) return;
        const count = Number($("#pos-cantidad")?.value || 0);
        if (!Number.isSafeInteger(count) || count <= 0) return notify("La cantidad debe ser un número entero mayor que cero.","error");
        if (!currentWarehouse()) return notify("Seleccioná un almacén antes de agregar productos.","error");
        if (Number(med.precioVenta) <= 0) return notify("Este medicamento no tiene un precio de venta válido.","error");
        const quantity = cartQuantity(med.id)+count;
        const available = totalStock(med.id);
        if (quantity > available) return notify(`No hay inventario suficiente para ${med.nombre}. Disponible: ${available}; solicitado: ${quantity}.`,"error");
        state.cart.set(Number(med.id),{id:Number(med.id),code:med.codigo,name:med.nombre,quantity,price:Number(med.precioVenta)});
        renderCart(); renderSelected(); notify("");
        $("#pos-buscar").value = "";
        $("#pos-cantidad").value = "1";
        state.selectedMedicine = null;
        state.lots = []; renderSelected();
        $("#pos-buscar").focus();
    }
    async function resolveDetails() {
        const details = [];
        // Reconsultar antes de registrar para evitar lotes obsoletos en el navegador.
        for (const item of state.cart.values()) {
            const lots = await allPages("/Lote/GetData",item.name,2000);
            state.lots = lots.filter(l => Number(l.medicamentoId) === item.id);
            const reserved = allocations(item.id,item.quantity);
            if (!reserved) throw new Error(`No hay suficientes unidades disponibles de ${item.name}. Actualizá existencias y volvé a intentarlo.`);
            for (const line of reserved) details.push({...line,precioUnitario:item.price});
        }
        return details;
    }
    function busy(value) {
        state.busy = value;
        $("[data-pos-save]") && ($("[data-pos-save]").disabled = value);
        $("[data-pos-clear]") && ($("[data-pos-clear]").disabled = value);
        renderSelected();
    }
    function resetSale() {
        state.cart.clear(); state.selectedMedicine = null; state.lots = [];
        if ($("#pos-buscar")) $("#pos-buscar").value = "";
        if ($("#pos-cantidad")) $("#pos-cantidad").value = "1";
        if ($("#pos-documento")) $("#pos-documento").value = makeDocument();
        if ($("#pos-fecha")) $("#pos-fecha").value = today();
        if ($("#pos-observaciones")) $("#pos-observaciones").value = "";
        renderSelected(); renderCart(); hideSuggestions();
    }
    async function saveSale() {
        if (state.busy) return;
        const client = Number($("#pos-cliente")?.value || 0);
        const warehouse = currentWarehouse();
        const doc = $("#pos-documento")?.value.trim() || "";
        const date = $("#pos-fecha")?.value;
        if (!client) return notify("Elegí un cliente. Para compras sin identificación, registrá una sola vez 'Consumidor final' en Clientes.","error");
        if (!warehouse) return notify("Seleccioná un almacén de la sucursal.","error");
        if (!state.cart.size) return notify("Agregá al menos un medicamento antes de registrar.","error");
        if (!doc || doc.length > 50) return notify("El número de documento no es válido.","error");
        if (!date) return notify("La fecha de venta es obligatoria.","error");
        busy(true);
        let created = null;
        try {
            const detalles = await resolveDetails();
            const payload = {clienteId:client,almacenId:warehouse,numeroDocumento:doc,fechaVenta:date,
                observaciones:$("#pos-observaciones")?.value.trim() || null,detalles};
            created = await request("/Venta/Create",{method:"POST",headers:{"Content-Type":"application/json"},body:JSON.stringify(payload)});
            // Borrador registrado: nunca reintentar Create automáticamente tras este punto.
            const confirmNow = canConfirm && Boolean($("#pos-auto-confirm")?.checked);
            if (confirmNow) {
                try {
                    await request(`/Venta/Confirmar?id=${encodeURIComponent(created.id)}`,{method:"POST"});
                    notify(`Venta ${created.numeroDocumento || doc} (ID ${created.id}) CONFIRMADA. El inventario se descontó. El Cajero debe registrar el cobro en Pagos.`,"success");
                } catch (error) {
                    notify(`La venta ${created.numeroDocumento || doc} (ID ${created.id}) quedó GUARDADA COMO BORRADOR, pero NO se confirmó: ${error.message}. Revisala en Historial de ventas. No repitás el registro.`,"warning");
                }
            } else {
                notify(`Venta ${created.numeroDocumento || doc} (ID ${created.id}) guardada como BORRADOR. Las existencias aún NO se descontaron.`,"warning");
            }
            resetSale();
            state.page = 0;
            await loadHistory();
        } catch (error) {
            notify(created ? `La venta ya fue creada (ID ${created.id}). No repitás el registro. ${error.message}` : error.message,"error");
        } finally {busy(false);}
    }
    const status = value => `<span class="fc-pos-status is-${escape(String(value||"").toLowerCase())}">${escape(value||"—")}</span>`;
    async function loadHistory() {
        const body = $("[data-pos-history]");
        if (!body) return;
        body.innerHTML = `<tr><td colspan="7" class="fc-empty-cell">Consultando ventas…</td></tr>`;
        const search = $("[data-pos-history-search]")?.value || "";
        try {
            const response = await tableData("/Venta/GetData",search,state.page*10,10);
            const data = rows(response);
            state.count = filtered(response);
            body.innerHTML = data.length ? data.map(item => {
                const draft = String(item.estado || "").toLowerCase() === "borrador";
                const actions = [];
                if (draft && canConfirm) actions.push(`<button type="button" data-pos-confirm="${Number(item.id)}">Confirmar</button>`);
                if (draft && canCancel) actions.push(`<button type="button" data-pos-cancel="${Number(item.id)}" class="danger">Anular</button>`);
                if (canPay && String(item.estado || "").toLowerCase() === "confirmada") actions.push(`<button type="button" data-pos-pay="${Number(item.id)}">Cobrar (ID ${Number(item.id)})</button>`);
                return `<tr><td>${Number(item.id)}</td><td>${escape(item.numeroDocumento)}</td><td>${escape(item.clienteNombre)}</td><td>${showDate(item.fechaVenta)}</td><td>${escape(pesos(item.total))}</td><td>${status(item.estado)}</td><td><div class="fc-pos-history-actions">${actions.join("") || "—"}</div></td></tr>`;
            }).join("") : `<tr><td colspan="7" class="fc-empty-cell">No hay ventas para mostrar.</td></tr>`;
            $("[data-pos-count]").textContent = `${state.count} registro(s)`;
            $("[data-pos-page]").textContent = `${state.page+1} / ${Math.max(1,Math.ceil(state.count/10))}`;
            $("[data-pos-prev]").disabled = state.page <= 0;
            $("[data-pos-next]").disabled = (state.page+1)*10 >= state.count;
        } catch (error) {body.innerHTML = `<tr><td colspan="7" class="fc-empty-cell">${escape(error.message)}</td></tr>`;notify(error.message,"error");}
    }
    async function actHistory(id, action) {
        if (state.busy) return;
        const text = action === "Confirmar"
            ? `¿Confirmar la venta ID ${id}? Se descontarán las existencias del inventario.`
            : `¿Anular el borrador ID ${id}?`;
        if (!window.confirm(text)) return;
        busy(true);
        try {
            const result = await request(`/Venta/${action}?id=${encodeURIComponent(id)}`,{method:"POST"});
            notify(result?.message || "Operación realizada.","success");
            await loadHistory();
        } catch (error) {notify(error.message,"error");} finally {busy(false);}
    }
    async function init() {
        if (canCreate) {
            resetSale();
            try {
                const [clients,warehouses] = await Promise.all([request("/Cliente/GetActivos"),request("/Almacen/GetActivos")]);
                state.clients = Array.isArray(clients) ? clients.filter(c => c.activo !== false) : [];
                state.warehouses = Array.isArray(warehouses) ? warehouses : [];
                fillSelect($("#pos-cliente"),state.clients,"Seleccionar cliente",c=>`${c.nombre}${c.codigo ? ` (${c.codigo})` : ""}`);
                fillSelect($("#pos-almacen"),state.warehouses,"Seleccionar almacén",a=>`${a.codigo||""} · ${a.nombre||""}`);
                // No seleccionar arbitrariamente a un cliente registrado.
                const consumer = state.clients.find(c => /^consumidor\s+final$/i.test(String(c.nombre||"").normalize("NFD").replace(/[\u0300-\u036f]/g,"")));
                if (consumer) $("#pos-cliente").value = String(consumer.id);
                else $("[data-pos-client-help]").textContent = "No existe Consumidor final. Crealo una vez en Clientes o elegí un cliente registrado.";
                if (state.warehouses.length === 1) {
                    $("#pos-almacen").value = String(state.warehouses[0].id);
                    $("[data-pos-warehouse-wrap]").hidden = true;
                }
                if (!state.warehouses.length) notify("No hay almacenes activos. Un administrador debe configurar un almacén antes de registrar ventas.","warning");
            } catch (error) { notify(`No se pudieron cargar los datos de la venta: ${error.message}`,"error"); }
            $("#pos-buscar").addEventListener("input", e => {
                state.selectedMedicine = null;state.lots = [];renderSelected();
                clearTimeout(state.searchTimer);
                const query = e.target.value;
                state.searchTimer = setTimeout(() => findMedicine(query),260);
            });
            $("#pos-buscar").addEventListener("keydown", e => {
                if (e.key === "Escape") hideSuggestions();
                if (e.key === "Enter") {
                    const first = $("[data-pos-result]");
                    if (first) {e.preventDefault();first.click();}
                }
            });
            $("[data-pos-suggestions]").addEventListener("click",e=>{
                const btn = e.target.closest("[data-pos-result]");
                if (!btn) return;
                const med = $("[data-pos-suggestions]")._items?.[Number(btn.dataset.posResult)];
                if (med) selectMedicine(med);
            });
            $("#pos-almacen").addEventListener("change",e=>{
                if (state.cart.size && !window.confirm("Cambiar el almacén vaciará los productos agregados. ¿Continuar?")) {
                    e.target.value = e.target.dataset.previous || "";return;
                }
                e.target.dataset.previous = e.target.value;
                state.cart.clear();state.lots = [];state.selectedMedicine = null;renderSelected();renderCart();
            });
            if (state.warehouses.length === 1) $("#pos-almacen").dataset.previous = $("#pos-almacen").value;
            $("[data-pos-add]").addEventListener("click",addMedicine);
            $("[data-pos-save]").addEventListener("click",saveSale);
            $("[data-pos-clear]").addEventListener("click",()=>{if(state.cart.size && !window.confirm("¿Vaciar los productos de esta venta?"))return;resetSale();notify("");});
            $("[data-pos-cart]").addEventListener("click",e=>{
                if (state.busy) return;
                const remove = e.target.closest("[data-pos-remove]");
                const plus = e.target.closest("[data-pos-plus]");
                const minus = e.target.closest("[data-pos-minus]");
                const btn = remove || plus || minus;
                if (!btn) return;
                const id = Number(btn.dataset.posRemove || btn.dataset.posPlus || btn.dataset.posMinus);
                const item = state.cart.get(id);
                if (!item) return;
                if (remove) state.cart.delete(id);
                else if (minus) {item.quantity--;if(item.quantity<1)state.cart.delete(id);}
                else if (plus) {
                    // Comprobar existencias nuevamente cuando se aumenta la cantidad.
                    const inc = async () => {
                        try {
                            const lots = await allPages("/Lote/GetData",item.name,2000);
                            state.lots = lots.filter(l=>Number(l.medicamentoId)===id);
                            if (item.quantity+1 > totalStock(id)) return notify("No hay más existencias disponibles de este producto.","error");
                            item.quantity++;renderCart();
                        } catch(error) {notify(error.message,"error");}
                    };
                    inc(); return;
                }
                renderCart();
            });
        }
        $("[data-pos-reload]")?.addEventListener("click",loadHistory);
        let searchTimer;
        $("[data-pos-history-search]")?.addEventListener("input",()=>{clearTimeout(searchTimer);searchTimer=setTimeout(()=>{state.page=0;loadHistory();},250);});
        $("[data-pos-prev]")?.addEventListener("click",()=>{if(state.page>0){state.page--;loadHistory();}});
        $("[data-pos-next]")?.addEventListener("click",()=>{if((state.page+1)*10<state.count){state.page++;loadHistory();}});
        $("[data-pos-history]")?.addEventListener("click",e=>{
            const confirm = e.target.closest("[data-pos-confirm]");
            const cancel = e.target.closest("[data-pos-cancel]");
            const pay = e.target.closest("[data-pos-pay]");
            if (confirm && canConfirm) actHistory(Number(confirm.dataset.posConfirm),"Confirmar");
            if (cancel && canCancel) actHistory(Number(cancel.dataset.posCancel),"Anular");
            if (pay && canPay) window.location.assign(`/Home?module=finanzas&tab=pagos&ventaId=${encodeURIComponent(pay.dataset.posPay)}`);
        });
        await loadHistory();
    }
    init().catch(error => notify(error.message,"error"));
})();

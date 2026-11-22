
(() => {
    "use strict";

    function initializeDashboard() {
        const root = document.querySelector("[data-fc-dashboard]");

        if (!root) return;

        const statusElement = root.querySelector(
            "[data-fc-dashboard-status]"
        );

        const refreshButton = root.querySelector(
            "[data-fc-refresh]"
        );

        const endpoints = {
            medicamentos: "/Medicamento/GetData",
            categorias: "/Categoria/GetData",
            lotes: "/Lote/GetData",
            compras: "/Compra/GetData",
            ventas: "/Venta/GetData"
        };

        const numberFormat = new Intl.NumberFormat("es-NI");

        function valueOf(object, lower, upper) {
            return object?.[lower] ?? object?.[upper];
        }

        function escapeHtml(value) {
            return String(value ?? "").replace(
                /[&<>"']/g,
                character => ({
                    "&": "&amp;",
                    "<": "&lt;",
                    ">": "&gt;",
                    '"': "&quot;",
                    "'": "&#39;"
                })[character]
            );
        }

        function getRows(response) {
            return response?.data
                ?? response?.Data
                ?? [];
        }

        function getTotal(response) {
            return Number(
                response?.recordsTotal
                ?? response?.RecordsTotal
                ?? response?.recordsFiltered
                ?? response?.RecordsFiltered
                ?? getRows(response).length
            );
        }

        function setKpi(key, value) {
            const element = root.querySelector(
                `[data-fc-kpi="${key}"]`
            );

            if (element) {
                element.textContent = value;
            }
        }

        function setStatus(message, isError = false) {
            if (!statusElement) return;

            statusElement.textContent = message;
            statusElement.classList.toggle("is-error", isError);
        }

        function buildRequest(length, orderBy = null, direction = "asc") {
            const body = new URLSearchParams();

            body.set("Draw", "1");
            body.set("Start", "0");
            body.set("Length", String(length));
            body.set("Search.Value", "");
            body.set("Search.Regex", "false");

            if (orderBy) {
                body.set("Order[0].Column", "0");
                body.set("Order[0].Dir", direction);

                body.set("Columns[0].Data", orderBy);
                body.set("Columns[0].Name", orderBy);
                body.set("Columns[0].Searchable", "false");
                body.set("Columns[0].Orderable", "true");
                body.set("Columns[0].Search.Value", "");
                body.set("Columns[0].Search.Regex", "false");
            }

            return body;
        }

        async function requestTable(url, length, orderBy, direction) {
            const response = await fetch(url, {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Accept": "application/json",
                    "Content-Type":
                        "application/x-www-form-urlencoded; charset=UTF-8",
                    "X-Requested-With": "XMLHttpRequest"
                },
                body: buildRequest(length, orderBy, direction)
            });

            if (response.status === 401) {
                window.location.assign("/Auth/Login");
                throw new Error("La sesión ha expirado.");
            }

            let result = {};

            try {
                result = await response.json();
            } catch {
                result = {};
            }

            if (response.status === 403) {
                throw new Error(
                    "No tienes permiso para consultar este módulo."
                );
            }

            if (!response.ok) {
                throw new Error(
                    result.message
                    ?? result.Message
                    ?? `Error HTTP ${response.status}`
                );
            }

            return result;
        }

        async function safeRequest(url, length, orderBy, direction) {
            try {
                const data = await requestTable(
                    url,
                    length,
                    orderBy,
                    direction
                );

                return {
                    success: true,
                    data
                };
            } catch (error) {
                console.error(
                    `Dashboard: error consultando ${url}`,
                    error
                );

                return {
                    success: false,
                    data: null,
                    error: error.message
                };
            }
        }

        function parseDate(value) {
            if (!value) return null;

            let date;

            // Evita desfases de zona horaria en fechas sin hora.
            if (typeof value === "string" && /^\d{4}-\d{2}-\d{2}/.test(value)) {
                const [year, month, day] = value.slice(0, 10).split("-").map(Number);
                // Fechas comerciales sin hora: no convertir la medianoche UTC.
                date = new Date(Date.UTC(year, month - 1, day, 12));
            } else {
                date = new Date(value);
            }

            return Number.isNaN(date.getTime()) ? null : date;
        }

        function formatDate(value) {
            const date = parseDate(value);

            if (!date) return "Fecha no disponible";

            return new Intl.DateTimeFormat("es-NI", {
                timeZone: "UTC", day: "2-digit", month: "short", year: "numeric"
            }).format(date);
        }

        function formatMoney(value) {
            const amount = Number(value ?? 0);

            return "C$ " + amount.toLocaleString("es-NI", {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
        }

        function renderEmpty(container, message) {
            if (!container) return;

            container.innerHTML = `
                <div class="fc-empty-state">
                    ${escapeHtml(message)}
                </div>
            `;
        }

        function renderAlerts(result) {
            const container = root.querySelector("[data-fc-alerts]");

            if (!container) return;

            if (!result.success) {
                renderEmpty(
                    container,
                    "No se pudo consultar el inventario. Verifica tus permisos."
                );

                return;
            }

            const response = result.data;
            const lots = getRows(response);

            // La fecha de vencimiento se evalúa por día comercial de Nicaragua.
            const parts = Object.fromEntries(new Intl.DateTimeFormat("en-US", {
                timeZone: "America/Managua", day: "2-digit", month: "2-digit", year: "numeric"
            }).formatToParts(new Date()).map(p => [p.type, p.value]));
            const today = new Date(Date.UTC(Number(parts.year), Number(parts.month) - 1, Number(parts.day), 12));
            const limit = new Date(today);
            limit.setUTCDate(limit.getUTCDate() + 90);

            const upcoming = lots
                .filter(lot => {
                    const active = valueOf(lot, "activo", "Activo");
                    const expiry = parseDate(
                        valueOf(lot, "fechaVencimiento", "FechaVencimiento")
                    );

                    if (active === false || !expiry) return false;

                    expiry.setUTCHours(12, 0, 0, 0);

                    return expiry >= today && expiry <= limit;
                })
                .sort((a, b) => {
                    return parseDate(
                        valueOf(a, "fechaVencimiento", "FechaVencimiento")
                    ) - parseDate(
                        valueOf(b, "fechaVencimiento", "FechaVencimiento")
                    );
                });

            setKpi("vencimientos", numberFormat.format(upcoming.length));

            if (upcoming.length === 0) {
                renderEmpty(
                    container,
                    "No hay lotes activos próximos a vencer en los próximos 90 días."
                );

                return;
            }

            container.innerHTML = upcoming.slice(0, 6).map(lot => {
                const medicine = valueOf(
                    lot, "medicamentoNombre", "MedicamentoNombre"
                );

                const lotNumber = valueOf(
                    lot, "numeroLote", "NumeroLote"
                );

                const warehouse = valueOf(
                    lot, "almacenNombre", "AlmacenNombre"
                );

                const expiry = valueOf(
                    lot, "fechaVencimiento", "FechaVencimiento"
                );

                const stock = valueOf(
                    lot, "existencias", "Existencias"
                );

                return `
                    <div class="fc-alert-item">
                        <div class="fc-alert-copy">
                            <strong>
                                ${escapeHtml(medicine || "Medicamento")}
                                · ${escapeHtml(lotNumber || "Sin lote")}
                            </strong>

                            <small>
                                ${escapeHtml(warehouse || "Sin almacén")}
                                · Vence ${escapeHtml(formatDate(expiry))}
                            </small>
                        </div>

                        <span class="fc-status-pill">
                            ${escapeHtml(numberFormat.format(Number(stock ?? 0)))}
                            uds.
                        </span>
                    </div>
                `;
            }).join("");
        }

        function renderActivity(purchasesResult, salesResult) {
            const container = root.querySelector("[data-fc-activity]");

            if (!container) return;

            const activities = [];

            if (purchasesResult.success) {
                getRows(purchasesResult.data).forEach(item => {
                    activities.push({
                        type: "Compra",
                        document: valueOf(
                            item, "numeroDocumento", "NumeroDocumento"
                        ),
                        detail: valueOf(
                            item, "proveedorNombre", "ProveedorNombre"
                        ),
                        date: valueOf(
                            item, "fechaCompra", "FechaCompra"
                        ),
                        total: valueOf(item, "total", "Total")
                    });
                });
            }

            if (salesResult.success) {
                getRows(salesResult.data).forEach(item => {
                    activities.push({
                        type: "Venta",
                        document: valueOf(
                            item, "numeroDocumento", "NumeroDocumento"
                        ),
                        detail: valueOf(
                            item, "clienteNombre", "ClienteNombre"
                        ),
                        date: valueOf(
                            item, "fechaVenta", "FechaVenta"
                        ),
                        total: valueOf(item, "total", "Total")
                    });
                });
            }

            activities.sort((a, b) => {
                return (parseDate(b.date)?.getTime() ?? 0)
                    - (parseDate(a.date)?.getTime() ?? 0);
            });

            const recent = activities
                .filter(item => parseDate(item.date))
                .slice(0, 6);

            if (recent.length === 0) {
                renderEmpty(
                    container,
                    "No hay compras o ventas disponibles para mostrar."
                );

                return;
            }

            container.innerHTML = recent.map(item => `
                <div class="fc-activity-item">
                    <div class="fc-activity-copy">
                        <strong>
                            ${escapeHtml(item.type)}
                            · ${escapeHtml(item.document || "Documento")}
                        </strong>

                        <small>
                            ${escapeHtml(item.detail || "Sin detalle")}
                            · ${escapeHtml(formatDate(item.date))}
                        </small>
                    </div>

                    <span class="fc-activity-amount">
                        ${escapeHtml(formatMoney(item.total))}
                    </span>
                </div>
            `).join("");
        }

        async function loadDashboard() {
            setStatus("Actualizando información...");

            if (refreshButton) {
                refreshButton.disabled = true;
            }

            try {
                const results = await Promise.all([
                    safeRequest(endpoints.medicamentos, 1),
                    safeRequest(endpoints.categorias, 1),
                    safeRequest(
                        endpoints.lotes,
                        -1,
                        "fechaVencimiento",
                        "asc"
                    ),
                    safeRequest(
                        endpoints.compras,
                        -1,
                        "fechaCompra",
                        "desc"
                    ),
                    safeRequest(
                        endpoints.ventas,
                        -1,
                        "fechaVenta",
                        "desc"
                    )
                ]);

                const [
                    medicinesResult,
                    categoriesResult,
                    lotsResult,
                    purchasesResult,
                    salesResult
                ] = results;

                setKpi(
                    "medicamentos",
                    medicinesResult.success
                        ? numberFormat.format(getTotal(medicinesResult.data))
                        : "—"
                );

                setKpi(
                    "categorias",
                    categoriesResult.success
                        ? numberFormat.format(getTotal(categoriesResult.data))
                        : "—"
                );

                setKpi(
                    "lotes",
                    lotsResult.success
                        ? numberFormat.format(getTotal(lotsResult.data))
                        : "—"
                );

                if (lotsResult.success) {
                    renderAlerts(lotsResult);
                } else {
                    setKpi("vencimientos", "—");

                    renderEmpty(
                        root.querySelector("[data-fc-alerts]"),
                        "No se pudo consultar el inventario."
                    );
                }

                renderActivity(purchasesResult, salesResult);

                const failed = results.filter(item => !item.success);

                if (failed.length > 0) {
                    setStatus(
                        "Algunos datos no pudieron consultarse. Verifica los permisos del usuario.",
                        true
                    );
                } else {
                    const lotTotal = getTotal(lotsResult.data);
                    const lotRows = getRows(lotsResult.data).length;

                    if (lotTotal > lotRows) {
                        setStatus(
                            `Información actualizada. Las alertas se calcularon sobre ${lotRows} de ${lotTotal} lotes.`
                        );
                    } else {
                        setStatus("Información actualizada correctamente.");
                    }
                }

            } catch (error) {
                console.error("Error general del Dashboard:", error);

                setStatus(
                    "No fue posible actualizar el Dashboard.",
                    true
                );
            } finally {
                if (refreshButton) {
                    refreshButton.disabled = false;
                }
            }
        }

        refreshButton?.addEventListener("click", loadDashboard);

        loadDashboard();
    }

    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initializeDashboard
        );
    } else {
        initializeDashboard();
    }
})();

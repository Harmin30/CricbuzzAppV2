(() => {
    const modalEl = document.getElementById("deleteModal");
    if (!modalEl) return;

    const modal = new bootstrap.Modal(modalEl);
    const messageEl = document.getElementById("deleteMessage");
    const form = document.getElementById("deleteForm");
    const hiddenInputs = document.getElementById("deleteHiddenInputs");

    function openDeleteModal({ controller, action, ids, message }) {
        messageEl.innerHTML = message;
        form.action = `/${controller}/${action}`;
        hiddenInputs.innerHTML = "";

        ids.forEach(id => {
            hiddenInputs.innerHTML +=
                `<input type="hidden" name="${action === "DeleteSelected" ? "selectedIds" : "id"}" value="${id}" />`;
        });

        modal.show();
    }

    // SINGLE DELETE
    document.addEventListener("click", e => {
        const btn = e.target.closest(".single-delete-btn");
        if (!btn) return;

        e.preventDefault();
        e.stopPropagation();

        openDeleteModal({
            controller: btn.dataset.controller,
            action: "DeleteConfirmed",
            ids: [btn.dataset.id],
            message: `Are you sure you want to delete <strong>${btn.dataset.name}</strong>?`
        });
    });

    // BULK DELETE
    document.addEventListener("submit", e => {
        if (!e.target.matches(".bulk-delete-form")) return;

        e.preventDefault();
        const checked = e.target.querySelectorAll(".row-checkbox:checked");
        if (checked.length === 0) return;

        openDeleteModal({
            controller: e.target.dataset.controller,
            action: "DeleteSelected",
            ids: [...checked].map(cb => cb.value),
            message: `Are you sure you want to delete <strong>${checked.length}</strong> item(s)?`
        });
    });
})();

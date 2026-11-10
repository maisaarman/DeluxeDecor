
(function () {
  const dlg = () => document.getElementById('inquiry-dialog');

  document.addEventListener('click', (e) => {
    const opener = e.target.closest('[data-inquiry]');
    if (opener) {
      const d = dlg();
      if (!d) return;
      const f = d.querySelector('#inq-form');
      f.reset();
      f.classList.remove('sent');
      f.elements['productId'].value = opener.dataset.productId || '';

      const name = opener.dataset.productName || '';
      d.querySelector('#inq-title').textContent = name;

      const msg = d.querySelector('.inq-msg');
      msg.textContent = '';
      msg.className = 'inq-msg';

      const wa = d.querySelector('#inq-wa');
      if (wa) {
        if (d.dataset.wa) {
          wa.href = 'https://wa.me/' + d.dataset.wa + '?text=' +
            encodeURIComponent((d.dataset.waMsg || '') + ' ' + name);
          wa.hidden = false;
        } else {
          wa.hidden = true;
        }
      }
      d.showModal();
      return;
    }

    const d = dlg();
    if (!d) return;
    if (e.target.closest('[data-inquiry-close]') || e.target === d) d.close();  // زر الإغلاق أو الضغط على الخلفية
  });

  document.addEventListener('submit', async (e) => {
    const f = e.target;
    if (f.id !== 'inq-form') return;
    e.preventDefault();

    const d = dlg();
    const msg = f.querySelector('.inq-msg');
    const btn = f.querySelector('[type=submit]');

    if (!f.checkValidity()) { f.reportValidity(); return; }

    btn.disabled = true;
    msg.className = 'inq-msg';
    msg.textContent = '';

    try {
      const pid = f.elements['productId'].value;
      const res = await fetch('/api/inquiry', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          productId: pid ? Number(pid) : null,
          kind: f.elements['kind'].value,
          name: f.elements['name'].value,
          email: f.elements['email'].value,
          message: f.elements['message'].value,
          website: f.elements['website'].value
        })
      });
      if (!res.ok) throw new Error('failed');

      f.classList.add('sent');
      msg.textContent = d.dataset.ok;
      msg.className = 'inq-msg ok';
      setTimeout(() => d.close(), 2600);
    } catch {
      msg.textContent = d.dataset.err;
      msg.className = 'inq-msg err';
    } finally {
      btn.disabled = false;
    }
  });
})();
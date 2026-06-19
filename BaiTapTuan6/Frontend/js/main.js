// Base API URL for the Web API
const apiUrl = 'https://localhost:7285/api/ProductApi';

// DOM Elements
const bookForm = document.getElementById('bookForm');
const bookIdInput = document.getElementById('bookId');
const bookNameInput = document.getElementById('bookName');
const bookPriceInput = document.getElementById('bookPrice');
const bookDescriptionInput = document.getElementById('bookDescription');
const btnSubmit = document.getElementById('btnSubmit');
const btnReset = document.getElementById('btnReset');
const formTitle = document.getElementById('formTitle');
const bookList = document.getElementById('bookList');
const btnRefresh = document.getElementById('btnRefresh');

// Detail Modal Elements
const detailModal = new bootstrap.Modal(document.getElementById('detailModal'));
const detailId = document.getElementById('detailId');
const detailName = document.getElementById('detailName');
const detailPrice = document.getElementById('detailPrice');
const detailDescription = document.getElementById('detailDescription');

// Load books on page load
document.addEventListener('DOMContentLoaded', () => {
    fetchProducts();
    
    // Bind form submit event
    bookForm.addEventListener('submit', handleFormSubmit);
    
    // Bind reset/cancel button event
    btnReset.addEventListener('click', resetForm);
    
    // Bind refresh button event
    btnRefresh.addEventListener('click', fetchProducts);
});

// Fetch all products from API (GET)
async function fetchProducts() {
    showTableLoading();
    try {
        const response = await fetch(apiUrl);
        if (!response.ok) throw new Error('Failed to load products');
        
        const products = await response.json();
        displayProducts(products);
    } catch (error) {
        console.error('Error fetching products:', error);
        showTableError(error.message);
    }
}

// Render product list into the HTML table
function displayProducts(products) {
    if (!products || products.length === 0) {
        bookList.innerHTML = `
            <tr>
                <td colspan="4" class="text-center py-5 text-muted">
                    <i class="bi bi-inbox fs-2 d-block mb-2"></i>
                    No books in inventory.
                </td>
            </tr>
        `;
        return;
    }
    
    bookList.innerHTML = '';
    products.forEach(product => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td class="text-muted fw-bold">#${product.id}</td>
            <td>
                <div class="fw-bold">${escapeHtml(product.name)}</div>
                <div class="small text-muted text-truncate" style="max-width: 250px;">${escapeHtml(product.description)}</div>
            </td>
            <td><span class="badge badge-price">${formatNumber(product.price)} đ</span></td>
            <td class="text-end">
                <button onclick="viewProduct(${product.id})" class="btn btn-outline-primary btn-sm action-btn me-1" title="View Details">
                    <i class="bi bi-eye"></i>
                </button>
                <button onclick="editProduct(${product.id})" class="btn btn-outline-warning btn-sm action-btn me-1" title="Edit Book">
                    <i class="bi bi-pencil"></i>
                </button>
                <button onclick="deleteProduct(${product.id})" class="btn btn-outline-danger btn-sm action-btn" title="Delete Book">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;
        bookList.appendChild(row);
    });
}

// Fetch single product and show in Modal (GET by ID)
async function viewProduct(id) {
    try {
        const response = await fetch(`${apiUrl}/${id}`);
        if (!response.ok) throw new Error('Product not found');
        
        const product = await response.json();
        
        // Populate modal fields
        detailId.textContent = product.id;
        detailName.textContent = product.name;
        detailPrice.textContent = `${formatNumber(product.price)} đ`;
        detailDescription.textContent = product.description;
        
        // Show modal
        detailModal.show();
    } catch (error) {
        alert('Error getting product details: ' + error.message);
    }
}

// Populate form for Editing (PUT prep)
async function editProduct(id) {
    try {
        const response = await fetch(`${apiUrl}/${id}`);
        if (!response.ok) throw new Error('Product not found');
        
        const product = await response.json();
        
        // Populate form fields
        bookIdInput.value = product.id;
        bookNameInput.value = product.name;
        bookPriceInput.value = product.price;
        bookDescriptionInput.value = product.description;
        
        // Set Form state to Update Mode
        formTitle.textContent = 'Edit Book Details';
        btnSubmit.innerHTML = '<i class="bi bi-check-circle me-2"></i>Update Book';
        btnSubmit.className = 'btn btn-success-gradient';
        btnReset.style.display = 'inline-block';
        
        // Scroll to form (for mobile view)
        bookForm.scrollIntoView({ behavior: 'smooth' });
    } catch (error) {
        alert('Error loading product details: ' + error.message);
    }
}

// Create or Update Product handler
async function handleFormSubmit(event) {
    event.preventDefault();
    
    // Simple client validation
    if (!bookForm.checkValidity()) {
        bookForm.reportValidity();
        return;
    }
    
    const id = parseInt(bookIdInput.value);
    const productData = {
        id: id,
        name: bookNameInput.value.trim(),
        price: parseFloat(bookPriceInput.value),
        description: bookDescriptionInput.value.trim()
    };
    
    const isEditing = id > 0;
    const url = isEditing ? `${apiUrl}/${id}` : apiUrl;
    const method = isEditing ? 'PUT' : 'POST';
    
    try {
        const response = await fetch(url, {
            method: method,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(productData)
        });
        
        if (!response.ok) {
            const errorMsg = await response.text();
            throw new Error(errorMsg || 'Action failed');
        }
        
        // Success
        resetForm();
        fetchProducts();
        
        // Notification
        const actionText = isEditing ? 'updated' : 'added';
        alert(`Product successfully ${actionText}!`);
        
    } catch (error) {
        alert(`Error executing operation: ${error.message}`);
    }
}

// Delete product from Database (DELETE)
async function deleteProduct(id) {
    if (!confirm('Are you sure you want to delete this book?')) return;
    
    try {
        const response = await fetch(`${apiUrl}/${id}`, {
            method: 'DELETE'
        });
        
        if (!response.ok) throw new Error('Failed to delete book');
        
        fetchProducts();
        alert('Book successfully deleted!');
    } catch (error) {
        alert('Error deleting book: ' + error.message);
    }
}

// Reset form fields and state
function resetForm() {
    bookForm.reset();
    bookIdInput.value = '0';
    formTitle.textContent = 'Add New Book';
    btnSubmit.innerHTML = '<i class="bi bi-plus-circle me-2"></i>Add Book';
    btnSubmit.className = 'btn btn-primary-gradient';
    btnReset.style.display = 'none';
}

// UI Helper methods
function showTableLoading() {
    bookList.innerHTML = `
        <tr>
            <td colspan="4" class="text-center py-5 text-muted">
                <div class="spinner-border text-primary spinner-border-sm me-2" role="status"></div>
                Updating inventory list...
            </td>
        </tr>
    `;
}

function showTableError(message) {
    bookList.innerHTML = `
        <tr>
            <td colspan="4" class="text-center py-5 text-danger">
                <i class="bi bi-exclamation-triangle-fill fs-2 d-block mb-2"></i>
                Error loading data: ${escapeHtml(message)}<br>
                <small class="text-muted">Make sure the backend server is running on port 5030.</small>
            </td>
        </tr>
    `;
}

function formatNumber(num) {
    return new Intl.NumberFormat('vi-VN').format(num);
}

function escapeHtml(str) {
    return str
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

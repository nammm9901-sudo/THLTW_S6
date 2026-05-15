const productsData = {
    "1": {
        name: "Luxury Velvet Sofa Heritage",
        price: "150.000.000 VNĐ",
        desc: "Sofa bọc nhung Italy cao cấp, khung gỗ sồi nguyên khối. Mang lại sự êm ái và vẻ đẹp cổ điển cho phòng khách biệt thự.",
        img: "https://images.unsplash.com/photo-1555041469-a586c61ea9bc",
        material: "Nhung Italy, Gỗ Sồi", origin: "Italy"
    },
    "2": {
        name: "Royal Gold Dining Table",
        price: "220.000.000 VNĐ",
        desc: "Bàn ăn hoàng gia với mặt đá Marble tự nhiên và chân bàn dát vàng 24k tỉ mỉ.",
        img: "https://vuongquocnoithat.vn/images/2017/04/17/Bo-ban-ghe-phong-an-phong-cach-hoang-gia%20GDT715+Y707-1.jpg",
        material: "Đá Marble, Vàng 24k", origin: "Châu Âu"
    },
    "3": {
        name: "Presidential Silk Bed",
        price: "180.000.000 VNĐ",
        desc: "Giường ngủ bọc lụa tơ tằm cao cấp, thiết kế công thái học giúp giấc ngủ sâu và thư giãn tối đa.",
        img: "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85",
        material: "Lụa tơ tằm, Gỗ Óc Chó", origin: "Pháp"
    },
    "4": {
        name: "Crystal Palace Lamp",
        price: "75.000.000 VNĐ",
        desc: "Đèn chùm pha lê cao cấp với hàng ngàn viên pha lê Swarovski tán sắc ánh sáng cực đẹp.",
        img: "https://tse2.mm.bing.net/th/id/OIP.uFIPfyJrMpdbFdXVrmSTnAHaHa?r=0&w=474&h=379&c=7&p=0",
        material: "Pha lê Swarovski, Đồng thau", origin: "Áo"
    },
    "5": {
        name: "Classic Wine Cabinet",
        price: "95.000.000 VNĐ",
        desc: "Tủ rượu bảo quản chuyên dụng với hệ thống kiểm soát độ ẩm và ánh sáng chuẩn quốc tế.",
        img: "https://media.noithatcaco.vn/uploads/product/thumb3/tu-ruou-go-mdf-canh-kinh-co-den-led-cao-cap-sang-trong-tr07-0123456789.jpg",
        material: "Gỗ Anh Đào, Kính cường lực", origin: "Đức"
    },
    "6": {
        name: "Premium Art Decor",
        price: "45.000.000 VNĐ",
        desc: "Tác phẩm decor treo tường bằng thép sơn tĩnh điện nghệ thuật, tạo điểm nhấn phá cách cho không gian.",
        img: "https://binhandecor.vn/wp-content/uploads/2025/01/tranh-sat-treo-tuong-decor-hoa-trang-sang-trong2.jpg",
        material: "Thép nghệ thuật", origin: "Việt Nam"
    }
};

// Logic lấy ID và hiển thị
const params = new URLSearchParams(window.location.search);
const id = params.get('id');

if (id && productsData[id]) {
    const p = productsData[id];
    document.getElementById("product-name").innerText = p.name;
    document.getElementById("product-price").innerText = p.price;
    document.getElementById("product-desc").innerText = p.desc;
    document.getElementById("product-image").src = p.img;
    document.getElementById("product-material").innerText = p.material;
    document.getElementById("product-origin").innerText = p.origin;
}
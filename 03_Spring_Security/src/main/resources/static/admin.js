let saved_id; // Gebruikt bij het updaten van een blogpost
let posts; // Een lijst van alle blogpost objecten die we hebben
let rest_url = "/posts";
const csrfToken = document.cookie.replace(/(?:(?:^|.*;\s*)XSRF-TOKEN\s*\=\s*([^;]*).*$)|^.*$/, '$1');


let clearFields = () => {
    document.getElementById("txtTitle").value = "";
    document.getElementById("txtContent").value = "";
};

let success = (message) => {
    console.log(message);
    document.getElementById("success_alert").innerText = message;
    document.getElementById("success_alert").style.display = "block";
    document.getElementById("fail_alert").style.display = "none";
};

let fail = (message) => {
    console.log(message);
    document.getElementById("fail_alert").innerText = message;
    document.getElementById("success_alert").style.display = "none";
    document.getElementById("fail_alert").style.display = "block";
};

let update = (title, content) => {
    fetch(rest_url+"/"+saved_id, {
        method: 'put',
        headers: { "content-type": "application/json", "X-XSRF-TOKEN": csrfToken },
        body: JSON.stringify({"id": saved_id, "title": title, "content": content})}
    )
            .then(() => {
                success("Blogpost updated");
                clearFields();
                refreshTable();
            })
            .catch(() => {
                fail("Error adding blogpost");
            })
};

let add = (title, content) => {
    // nieuwe toevoegen -> POST request
    fetch(rest_url, {
        method: 'post',
        headers: { "content-type": "application/json", "X-XSRF-TOKEN": csrfToken },
        body: JSON.stringify({"title": title, "content": content})
    })
            .then((result) => {
                if (result.status === 201) {
                    success("Blogpost added.");
                } else {
                    fail("Error adding blogpost");
                }
                clearFields();
                refreshTable();
            })
            .catch(() => {
                fail("Error adding blogpost");
            });
};

let remove = (id) => {
    // Ajax call om een specifieke blogpost te verwijderen
    fetch(rest_url + "/" + id, {
        method: 'delete',
        headers: { 'X-XSRF-TOKEN': csrfToken },
    })
            .then((result) => {
                if (result.status === 204) {
                    success("Blogpost deleted.");
                } else {
                    fail("Error adding blogpost");
                }
                refreshTable();
            })
            .catch(() => {
                fail("Error deleting blogpost");
            });
};

let formSubmit = (event) => {
    // Ervoor zorgen dat er geen pagina refresh is. Alternatief is om geen
    // formulier te gebruiken en de click event af te handelen i.p.v. de
    // submit event
    event.preventDefault();

    // Inhoud van de tekstvelden opvragen
    let title = document.getElementById("txtTitle").value;
    let content = document.getElementById("txtContent").value;

    // We komen hier ofwel omdat we een nieuwe blogpost willen maken ofwel
    // omdat we een bestaande blogpost willen updaten
    if (saved_id !== undefined) {
        update(title, content);

    } else {
        add(title, content);
    }
};

let refreshTable = () => {
    fetch(rest_url, {headers: { 'X-XSRF-TOKEN': csrfToken }})
            .then(data => data.json())
            .then((result) => {
                document.getElementsByTagName("tbody")[0].innerHTML = "";
                posts = result;
                console.log(result);

                // Voor elke post een nieuwe rij aanmaken in de tabel
                for (let post of result) {
                    // Hier gebruiken we een speciaal attribuut: data-*
                    // https://www.w3schools.com/tags/att_global_data.asp
                    // Dit laat ons toe om arbitraire data toe te voegen aan html elementen
                    // Deze attributen beginnen altijd met "data-".
                    // In dit geval gebruiken we het om de id van de blogpost toe te voegen aan
                    // de tr. Dit maakt het eenvoudiger om later op het moment dat we op
                    // de delete of update knop klikken het blogpost object terug te vinden.
                    let content = `
                        <tr data-id=${post.id}>
                            <td>${post.title}</td>
                            <td><button class="btn_delete" onclick="deleteHandler(this)">delete</button></td>
                            <td><button class="btn_edit" onclick="editHandler(this)">edit</button></td>
                        </tr>`;
                    // Nieuwe rijd toevoegen binnen de tabel.
                    document.getElementsByTagName("tbody")[0].insertAdjacentHTML("beforeEnd", content);
                }
            })
            .catch(() => {
                fail("Error refreshing posts");
            });
};


let deleteHandler = (e) => {
    console.log(e.parentElement.parentElement);
    let id = e.parentElement.parentElement.dataset.id;
    console.log("delete " + id);
    remove(id);
};

let editHandler = (e) => {
    console.log(e.parentElement.parentElement);
    let id = e.parentElement.parentElement.dataset.id;
    console.log("edit " + id);
    document.getElementById("txtTitle").value = getPostById(id).title;
    document.getElementById("txtContent").value = getPostById(id).content;
    saved_id = id;
};


let getPostById = (id) => {
    // Zoek in de array posts naar de blogpost met deze id
    id = parseInt(id); // converteer string -> int
    return posts.filter(p => p.id === id)[0];
};


let main = () => {
    // alerts verbergen
    document.getElementById("fail_alert").style.display = "none";
    document.getElementById("success_alert").style.display = "none";

    // Submit van het formulier afhandelen
    document.getElementById("frm").addEventListener("submit", formSubmit);

    // reset van het formulier afhandelen
    document.getElementById("frm").addEventListener("reset", (event) => {
        saved_id = undefined;
    });

    refreshTable();
};


main();

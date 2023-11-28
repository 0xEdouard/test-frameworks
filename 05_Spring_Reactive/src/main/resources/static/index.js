let url = "/posts";
let main = () => {
    console.log("main");
    fetch(url)
        .then(data => data.json())
        .then(data => showPosts(data));
    const changeStream = new EventSource("/stream/posts");
    changeStream.onmessage = showUpdate;
};

let search = () => {
    let keyword = document.getElementById("search").value;
    console.log("searching: " + keyword);
    fetch(url + "/search?q=" + keyword)
        .then(response => response.json())
        .then(data => showPosts(data));
}

let showPosts = (data) => {
    console.log(data);
    let main = document.getElementsByTagName("main")[0];
    main.innerHTML = "";

    for (let post of data) {
        let article = `
                        <div class="card mb-4">
                            <div class="card-header">
                                ${post.title}
                            </div>
                            <div class="card-body">
                                <p class="card-text">${post.content}</p>
                                <a href="#" class="btn btn-primary">Read more</a>
                            </div>
                        </div>
                    `;
        main.insertAdjacentHTML("beforeEnd", article);
    }
};

let showUpdate = (data) => {
    console.log(data);
    let post = JSON.parse(data.data);
    let alertsPlaceHolder = document.getElementById("alerts");
    let update = `
    <div id="${post.id}" class="alert alert-info alert-dismissible fade show" role="alert">
      <strong>New Update!</strong> ${post.title}
      <button type="button" class="close" data-dismiss="alert" aria-label="Close">
        <span aria-hidden="true">&times;</span>
      </button>
    </div>`;
    alertsPlaceHolder.insertAdjacentHTML("afterBegin", update);
    setTimeout(() => $("#" + post.id).alert('close'), 2000);
}

main();

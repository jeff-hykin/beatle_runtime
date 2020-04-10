<script>
import Vue from "vue"

let getIndentLength = (string) => {
    return string.match(/^\n*( *)/)[1].length
}
let removeIndent = (string, indentLength) => {
    return string.replace(new RegExp(`(\n|^) {0,${indentLength}}`,"g"), "$1")
}

export default Vue.component("markdown", {
    render: function(createElement) {
        let nodes = []
        if (this.$slots) {
            let indentLength = 0
            // get the indent
            if (this.$slots.default[0] && this.$slots.default[0].text) {
                indentLength = getIndentLength(this.$slots.default[0].text)
            }
            let breakSequence = false
            for (let each of this.$slots.default) {
                if (each.text) {
                    breakSequence = false
                    let source = removeIndent(each.text, indentLength)
                    nodes.push(
                        createElement(
                            "vue-simple-markdown", // tag name
                            {
                                props: { source },
                                class: "markdown-container",
                                style: {
                                    width: "100%",
                                },
                            }
                        )
                    )
                } else if (each.tag == "br") {
                    // if its the second break then add a space
                    if (breakSequence) {
                        nodes.push(
                            createElement(
                                "div",
                                {
                                    style: {
                                        height: "1rem"
                                    }
                                }
                            )
                        )
                    }
                    breakSequence = true
                } else if (each.tag != "br") {
                    breakSequence = false
                    nodes.push(each)
                }
            }
            return createElement(
                "div",
                {
                    class: "markdown-wrapper",
                    style: {
                        display: "flex",
                        flexDirection: "column",
                        justifyContent: "flex-start",
                        alignItems: "center",
                    },
                },
                [
                    createElement(
                        "div",
                        {
                            style: {
                                maxWidth: "80rem",
                                width: "-webkit-fill-available",
                                textAlign: "left",
                            },
                        },
                        nodes
                    ),
                ]
            )
        }
    },
})

</script>
<style scoped lang="scss">
::v-deep {
    img {
        max-width: 100%;
    }
    li {
        margin-left: 2rem;
    }
    h1 {
        font-size: 3.84rem;
    }
    h4 {
        font-size: 1.92rem;
    }
} 
</style>